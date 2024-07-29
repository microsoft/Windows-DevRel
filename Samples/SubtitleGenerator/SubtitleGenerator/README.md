# AI Powered Subtitle Generator App [Sample]

This sample is a simple video subtitles generator app that uses local APIs and models to provide AI powered features. The app is built using WinUI3.

<img alt="App Main Window" src="https://github.com/microsoft/Windows-DevRel/assets/6115884/a577a7ba-7a27-45ed-b743-0efe666cec90" width="500">


## Set Up

You will need to have Visual Studio installed with the latest workloads for WinAppSDK and WinUI 3 development. You can find instructions on how to set up your environment [here.](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b#install-visual-studio)

Clone the repository and open the solution in Visual Studio. Before you can get started exploring the sample, you will need to download the ML model files required for the project and place them in the `Assets/Models` folder.

## Downloading Sliero VAD
The Sliero Voice Activity Detection model can be downloaded from the following link:
- https://github.com/snakers4/silero-vad 

This is the model we use for smart chunking of audio and the only file you will need is the `/files/sliero_vad.onnx` file. 

This should be placed under a folder called `Models` under the `Assets` folder.

## Downloading Whisper
Whisper models can be downloaded from HF:
- https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/tree/main

**Or** you can manually generate them with [Olive](https://github.com/microsoft/OLive).

This can all be done from the command line and only requires Python as a dependency, to get your model, follow these steps:

1. Clone the Olive repository and navigate to the Whisper example folder:
```
git clone https://github.com/microsoft/Olive
cd Olive/examples/whisper
```

2. Install the required packages:
```
pip install olive-ai
python -m pip install -r requirements.txt
pip install onnxruntime==1.18.0
pip install onnxruntime_extensions
```

3. Prepare the Whisper model
```
python prepare_whisper_configs.py --model_name openai/whisper-small --multilingual --enable_timestamps 
```

4. Run the Olive workflow to generate the optimized model
```
olive run --config whisper_cpu_int8.json --setup
olive run --config whisper_cpu_int8.json
```

5. The generated model will be in the \models\conversion-transformers_optimization-onnx_dynamic_quantization-insert_beam_search-prepost folder. 

6. Rename the model from `whisper_cpu_int8_cpu-cpu_model.onnx` to `whisper_small_int8_cpu_ort_1.18.0.onnx` and place it in the `Assets/Models/Whisper` folder.

7. Do the same for the `whisper-tiny` and `whisper-medium` models.

## Troubleshooting

### Path name too long
You might run into an issue if you clone the repo in a location that will make the path too long to some of the generated binaries. Recomendation is to place the repo closer to the root of the drive and rename the repo folder name to something shorter. Alternatively, you can change the settings in Windows to support long paths
https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=registry#enable-long-paths-in-windows-10-version-1607-and-later .

## [Contributing and Trademarks](https://github.com/microsoft/Windows-DevRel?tab=readme-ov-file#contributing)
