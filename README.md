A heuristic solution to a multidimensional multiple-choice [knapsack problem](https://en.wikipedia.org/wiki/Knapsack_problem) using a compute shader to offload the work to the GPU.

![thumb2](https://github.com/jhntrnr/compute-shader-knapsack/assets/90057903/aa09cc8d-bd6c-4d94-b67d-daf6261ce881)

Problem definition:
There are four 1024x1024 images initialized with a random RGB color set in each pixel. Pick exactly one pixel from each image such that the total *Red* value of selected pixels is maximized, the total *Green* value of selected pixels is above some threshold, and the total *Blue* value of selected pixels is below another threshold.

Solution explanation:
The solution in this project is defined as the center 4 pixels in the resulting render texture (the bottom-right pixel of the top-left quadrant, bottom-left pixel of the top-right quadrant, etc).

The solution found by the compute shader is not an exact one; the shader is not guaranteed to find an optimal answer. It uses a heuristic function to score pixels along with sorting algorithms to try and find good solutions quickly. It optionally applies a "temperature" value to pixels as a form of simulated annealing to escape local minima and find better solutions.

Demonstration: https://youtu.be/PMCOIaDS8vY

Full Explanation: https://youtu.be/wf4sxYKgxqY

Note that this project may not work properly on all hardware. See Unity's compute shader documentation for details: https://docs.unity3d.com/Manual/class-ComputeShader.html
