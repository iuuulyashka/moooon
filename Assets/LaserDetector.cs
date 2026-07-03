using UnityEngine;
using System.Collections;

public class LaserDetector : MonoBehaviour
{
    [Header("Нейросеть")]
    public Unity.InferenceEngine.ModelAsset modelAsset;
    public float confidenceThreshold = 0.5f;
    public float detectionInterval = 0.5f;

    [Header("Лазер")]
    public LineRenderer laserLine;
    public Transform laserOrigin;
    public float laserDuration = 0.3f;

    private Unity.InferenceEngine.Worker worker;
    private Camera cam;
    private bool isShooting = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        var model = Unity.InferenceEngine.ModelLoader.Load(modelAsset);
        worker = new Unity.InferenceEngine.Worker(model, Unity.InferenceEngine.BackendType.GPUCompute);

        if (laserLine != null)
        {
            laserLine.enabled = false;
            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.01f;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startColor = Color.red;
            laserLine.endColor = new Color(1f, 0f, 0f, 0f);
        }

        InvokeRepeating(nameof(Detect), 1f, detectionInterval);
    }

    void Detect()
    {
        if (isShooting) return;

        RenderTexture rt = new RenderTexture(640, 640, 0);
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = null;

        Texture2D tex = new Texture2D(640, 640, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, 640, 640), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        var pixels = tex.GetPixels32();
        float[] inputData = new float[3 * 640 * 640];
        for (int y = 0; y < 640; y++)
        {
            for (int x = 0; x < 640; x++)
            {
                int idx = y * 640 + x;
                var p = pixels[(639 - y) * 640 + x];
                inputData[0 * 640 * 640 + idx] = p.r / 255f;
                inputData[1 * 640 * 640 + idx] = p.g / 255f;
                inputData[2 * 640 * 640 + idx] = p.b / 255f;
            }
        }

        using var tensor = new Unity.InferenceEngine.Tensor<float>(new Unity.InferenceEngine.TensorShape(1, 3, 640, 640), inputData);
        worker.Schedule(tensor);

        using var output = worker.PeekOutput() as Unity.InferenceEngine.Tensor<float>;
        var data = output.DownloadToArray();

        Destroy(rt);
        Destroy(tex);

        int numDetections = 8400;
        int numValues = data.Length / numDetections;
        float bestConf = 0f;
        float bestX = 0f, bestY = 0f;

        for (int i = 0; i < numDetections; i++)
        {
            float maxClassScore = 0f;
            for (int c = 4; c < numValues; c++)
            {
                float score = data[c * numDetections + i];
                if (score > maxClassScore) maxClassScore = score;
            }

            if (maxClassScore > confidenceThreshold && maxClassScore > bestConf)
            {
                bestConf = maxClassScore;
                bestX = data[0 * numDetections + i];
                bestY = data[1 * numDetections + i];
            }
        }

        Debug.Log("Лучшая уверенность: " + bestConf);

        if (bestConf > confidenceThreshold)
        {
            float screenX = bestX / 640f;
            float screenY = 1f - bestY / 640f;
            Ray ray = cam.ViewportPointToRay(new Vector3(screenX, screenY, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log("Raycast попал в: " + hit.collider.gameObject.name + " тег: " + hit.collider.gameObject.tag);
                GameObject target = hit.collider.gameObject;
                if (target.CompareTag("RedBall"))
                {
                    StartCoroutine(ShootLaser(hit.point, target));
                }
            }
            else
            {
                Debug.Log("Raycast никуда не попал");
            }
        }
    }

    IEnumerator ShootLaser(Vector3 targetPos, GameObject target)
    {
        isShooting = true;

        if (laserLine != null && laserOrigin != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, laserOrigin.position);
            laserLine.SetPosition(1, targetPos);
        }

        yield return new WaitForSeconds(laserDuration);

        if (target != null) Destroy(target);
        if (laserLine != null) laserLine.enabled = false;

        isShooting = false;
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
}