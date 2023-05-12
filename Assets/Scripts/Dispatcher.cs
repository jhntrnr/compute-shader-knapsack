using UnityEngine;
using UnityEngine.UI;

public class Dispatcher : MonoBehaviour
{
    [Header("Simulation Settings")]
    [Range(0.001f, 3f)]
    public float gThreshold;
    [Range(0.001f, 3f)]
    public float bThreshold;
    [Range(0f, .35f)]
    public float temperatureMult;
    public bool usingAlpha;
    public bool useRandomTemperature;
    public bool usePulsingTemperature;
    private bool tempInc;

    private RenderTexture ArrayAIn;
    private RenderTexture ArrayAOut;

    private RenderTexture ArrayBIn;
    private RenderTexture ArrayBOut;

    private RenderTexture ArrayCIn;
    private RenderTexture ArrayCOut;

    private RenderTexture ArrayDIn;
    private RenderTexture ArrayDOut;

    private RenderTexture Result;

    private int DrawResult;
    private int ArrayInitializer;
    private int SortingStep;
    private int HeuristicStep;
    private bool swapTextures;
    private float rngSeed;
    private int iterationCount = 0;
    private bool xyFlip;

    [Header("Object References")]
    public Texture startingTexture;
    public Material material;

    private int width = 1024;
    private int height = 1024;

    public ComputeShader Solver;

    public Button randomTempTog;
    public Button pulseTempTog;
    public Button scoreViewTog;
    public Slider gThreshSlider;
    public Slider bThreshSlider;
    public Slider tempSlider;
    void Start()
    {
        RestartSortSimulation();
    }

    void RestartSortSimulation()
    {
        gThreshold = gThreshSlider.value;
        bThreshold = bThreshSlider.value;
        temperatureMult = tempSlider.value;
        rngSeed = Random.Range(0f, 1f);
        InitializeTextures();
        InitializeComputeShader();
        Solver.SetTexture(ArrayInitializer, "ArrayAOut", ArrayAIn);
        Solver.SetTexture(ArrayInitializer, "ArrayBOut", ArrayBIn);
        Solver.SetTexture(ArrayInitializer, "ArrayCOut", ArrayCIn);
        Solver.SetTexture(ArrayInitializer, "ArrayDOut", ArrayDIn);
        Solver.SetTexture(DrawResult, "ArrayAIn", ArrayAOut);

        Solver.SetTexture(DrawResult, "ArrayAOut", ArrayAIn);
        Solver.SetTexture(DrawResult, "ArrayBOut", ArrayBIn);
        Solver.SetTexture(DrawResult, "ArrayCOut", ArrayCIn);
        Solver.SetTexture(DrawResult, "ArrayDOut", ArrayDIn);
        Solver.Dispatch(ArrayInitializer, width / 8, height / 8, 1);
        Solver.Dispatch(DrawResult, width / 4, height / 4, 1);
        Graphics.Blit(ArrayAIn, ArrayAOut);
        Graphics.Blit(ArrayBIn, ArrayBOut);
        Graphics.Blit(ArrayCIn, ArrayCOut);
        Graphics.Blit(ArrayDIn, ArrayDOut);
    }

    void InitializeTextures()
    {
        ArrayAIn = new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
        ArrayAIn.wrapMode = TextureWrapMode.Repeat;
        ArrayAIn.enableRandomWrite = true;
        ArrayAIn.filterMode = FilterMode.Point;
        ArrayAIn.useMipMap = false;
        ArrayAIn.Create();

        ArrayAOut = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        ArrayAOut.wrapMode = TextureWrapMode.Repeat;
        ArrayAOut.enableRandomWrite = true;
        ArrayAOut.filterMode = FilterMode.Point;
        ArrayAOut.useMipMap = false;
        ArrayAOut.Create();

        ArrayBIn = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        ArrayBIn.wrapMode = TextureWrapMode.Clamp;
        ArrayBIn.enableRandomWrite = true;
        ArrayBIn.filterMode = FilterMode.Point;
        ArrayBIn.useMipMap = false;
        ArrayBIn.Create();

        ArrayBOut = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        ArrayBOut.wrapMode = TextureWrapMode.Clamp;
        ArrayBOut.enableRandomWrite = true;
        ArrayBOut.filterMode = FilterMode.Point;
        ArrayBOut.useMipMap = false;
        ArrayBOut.Create();

        ArrayCIn = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        ArrayCIn.wrapMode = TextureWrapMode.Clamp;
        ArrayCIn.enableRandomWrite = true;
        ArrayCIn.filterMode = FilterMode.Point;
        ArrayCIn.useMipMap = false;
        ArrayCIn.Create();

        ArrayCOut = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        ArrayCOut.wrapMode = TextureWrapMode.Clamp;
        ArrayCOut.enableRandomWrite = true;
        ArrayCOut.filterMode = FilterMode.Point;
        ArrayCOut.useMipMap = false;
        ArrayCOut.Create();

        ArrayDIn = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        ArrayDIn.wrapMode = TextureWrapMode.Clamp;
        ArrayDIn.enableRandomWrite = true;
        ArrayDIn.filterMode = FilterMode.Point;
        ArrayDIn.useMipMap = false;
        ArrayDIn.Create();

        ArrayDOut = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        ArrayDOut.wrapMode = TextureWrapMode.Clamp;
        ArrayDOut.enableRandomWrite = true;
        ArrayDOut.filterMode = FilterMode.Point;
        ArrayDOut.useMipMap = false;
        ArrayDOut.Create();

        Result = new RenderTexture(2048, 2048, 24);
        Result.wrapMode = TextureWrapMode.Clamp;
        Result.enableRandomWrite = true;
        Result.filterMode = FilterMode.Bilinear;
        Result.useMipMap = false;
        Result.Create();

        Graphics.Blit(startingTexture, Result);
        material.SetTexture("_MainTex", Result);
    }
    void InitializeComputeShader()
    {
        DrawResult = Solver.FindKernel("DrawResult");
        ArrayInitializer = Solver.FindKernel("ArrayInitializer");
        SortingStep = Solver.FindKernel("SortingStep");
        HeuristicStep = Solver.FindKernel("HeuristicStep");
        Solver.SetBool("usingAlpha", usingAlpha);

        Solver.SetFloat("yThreshold", gThreshold);
        Solver.SetFloat("zThreshold", bThreshold);
        Solver.SetFloat("temperatureMult", temperatureMult);
        Solver.SetInt("width", width);
        Solver.SetInt("height", height);
        Solver.SetFloat("rngSeed", rngSeed);

        Solver.SetTexture(DrawResult, "Result", Result);

        iterationCount = 0;
        xyFlip = false;
        swapTextures = true;
        
        SwapInOut();
    }

    void SwapInOut()
    {
        if(iterationCount == 1)
        {
            xyFlip = !xyFlip;
            iterationCount = -1;
        }
        rngSeed = 0;
        Solver.SetFloat("rngSeed", rngSeed);
        iterationCount += 1;
        Solver.SetInt("iterationFlipper", iterationCount);
        Solver.SetBool("xyFlip", xyFlip);
        if (!swapTextures)
        {
            Solver.SetTexture(HeuristicStep, "ArrayAIn", ArrayAIn);
            Solver.SetTexture(HeuristicStep, "ArrayAOut", ArrayAOut);
            Solver.SetTexture(HeuristicStep, "ArrayBIn", ArrayBIn);
            Solver.SetTexture(HeuristicStep, "ArrayBOut", ArrayBOut);
            Solver.SetTexture(HeuristicStep, "ArrayCIn", ArrayCIn);
            Solver.SetTexture(HeuristicStep, "ArrayCOut", ArrayCOut);
            Solver.SetTexture(HeuristicStep, "ArrayDIn", ArrayDIn);
            Solver.SetTexture(HeuristicStep, "ArrayDOut", ArrayDOut);

            Solver.SetTexture(SortingStep, "ArrayAIn", ArrayAOut);
            Solver.SetTexture(SortingStep, "ArrayAOut", ArrayAIn);
            Solver.SetTexture(SortingStep, "ArrayBIn", ArrayBOut);
            Solver.SetTexture(SortingStep, "ArrayBOut", ArrayBIn);
            Solver.SetTexture(SortingStep, "ArrayCIn", ArrayCOut);
            Solver.SetTexture(SortingStep, "ArrayCOut", ArrayCIn);
            Solver.SetTexture(SortingStep, "ArrayDIn", ArrayDOut);
            Solver.SetTexture(SortingStep, "ArrayDOut", ArrayDIn);

            Solver.SetTexture(DrawResult, "ArrayAOut", ArrayAIn);
            Solver.SetTexture(DrawResult, "ArrayBOut", ArrayBIn);
            Solver.SetTexture(DrawResult, "ArrayCOut", ArrayCIn);
            Solver.SetTexture(DrawResult, "ArrayDOut", ArrayDIn);
        }
        else
        {
            Solver.SetTexture(HeuristicStep, "ArrayAIn", ArrayAOut);
            Solver.SetTexture(HeuristicStep, "ArrayAOut", ArrayAIn);
            Solver.SetTexture(HeuristicStep, "ArrayBIn", ArrayBOut);
            Solver.SetTexture(HeuristicStep, "ArrayBOut", ArrayBIn);
            Solver.SetTexture(HeuristicStep, "ArrayCIn", ArrayCOut);
            Solver.SetTexture(HeuristicStep, "ArrayCOut", ArrayCIn);
            Solver.SetTexture(HeuristicStep, "ArrayDIn", ArrayDOut);
            Solver.SetTexture(HeuristicStep, "ArrayDOut", ArrayDIn);

            Solver.SetTexture(SortingStep, "ArrayAIn", ArrayAIn);
            Solver.SetTexture(SortingStep, "ArrayAOut", ArrayAOut);
            Solver.SetTexture(SortingStep, "ArrayBIn", ArrayBIn);
            Solver.SetTexture(SortingStep, "ArrayBOut", ArrayBOut);
            Solver.SetTexture(SortingStep, "ArrayCIn", ArrayCIn);
            Solver.SetTexture(SortingStep, "ArrayCOut", ArrayCOut);
            Solver.SetTexture(SortingStep, "ArrayDIn", ArrayDIn);
            Solver.SetTexture(SortingStep, "ArrayDOut", ArrayDOut);

            Solver.SetTexture(DrawResult, "ArrayAOut", ArrayAOut);
            Solver.SetTexture(DrawResult, "ArrayBOut", ArrayBOut);
            Solver.SetTexture(DrawResult, "ArrayCOut", ArrayCOut);
            Solver.SetTexture(DrawResult, "ArrayDOut", ArrayDOut);
        }
        swapTextures = !swapTextures;
    }

    void Update()
    {
        PerformSimulationStep();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartSortSimulation();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            usingAlpha = !usingAlpha;
            Solver.SetBool("usingAlpha", usingAlpha);
        }
    }
    void PerformSimulationStep()
    {
        if (swapTextures)
        {

            Graphics.Blit(ArrayAOut, ArrayAIn);
            Graphics.Blit(ArrayBOut, ArrayBIn);
            Graphics.Blit(ArrayCOut, ArrayCIn);
            Graphics.Blit(ArrayDOut, ArrayDIn);
        }
        else
        {
            Graphics.Blit(ArrayAIn, ArrayAOut);
            Graphics.Blit(ArrayBIn, ArrayBOut);
            Graphics.Blit(ArrayCIn, ArrayCOut);
            Graphics.Blit(ArrayDIn, ArrayDOut);
        }
        if (useRandomTemperature)
        {
            temperatureMult = Random.Range(0f, 0.35f);
            
            Solver.SetFloat("temperatureMult", temperatureMult);
        }
        if (usePulsingTemperature)
        {
            if (tempInc)
            {
                if(temperatureMult < 0.35f)
                {
                    temperatureMult += 0.001f;
                }
                else
                {
                    tempInc = !tempInc;
                }
            }
            else
            {
                if (temperatureMult > 0.001f)
                {
                    temperatureMult -= 0.001f;
                }
                else
                {
                    tempInc = !tempInc;
                }
            }
            Solver.SetFloat("temperatureMult", temperatureMult);
        }
        tempSlider.value = temperatureMult;
        gThreshSlider.value = gThreshold;
        bThreshSlider.value = bThreshold;
        Solver.Dispatch(HeuristicStep, width / 8, height / 8, 1);
        Solver.Dispatch(SortingStep, width / 8, height / 8, 1);
        Solver.Dispatch(DrawResult, width / 4, height / 4, 1);
        SwapInOut();
    }

    private void OnValidate()
    {
        Solver.SetFloat("yThreshold", gThreshold);
        Solver.SetFloat("zThreshold", bThreshold);
        Solver.SetFloat("temperatureMult", temperatureMult);
        
    }

    public void ToggleViewMode()
    {
        usingAlpha = !usingAlpha;
        Solver.SetBool("usingAlpha", usingAlpha);
    }

    public void SetPulsingTempMode()
    {
        if (useRandomTemperature)
        {
            useRandomTemperature = false;
        }
        usePulsingTemperature = !usePulsingTemperature;
    }

    public void SetRandomTempMode()
    {
        if (usePulsingTemperature)
        {
            usePulsingTemperature = false;
        }
        useRandomTemperature = !useRandomTemperature;
    }

    public void GSliderChanged()
    {
        gThreshold = gThreshSlider.value;
        Solver.SetFloat("yThreshold", gThreshold);

    }

    public void BSliderChanged()
    {
        bThreshold = bThreshSlider.value;
        Solver.SetFloat("zThreshold", bThreshold);

    }

    public void TempSliderChanged()
    {
        temperatureMult = tempSlider.value;
        Solver.SetFloat("temperatureMult", temperatureMult);
    }
}
