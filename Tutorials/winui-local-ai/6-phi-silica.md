# Foundry
TODO: update naming

Windows Copilot Runtime provides AI-powered features and APIs on Copilot+ PCs. These features are in active development and run locally in the background at all times.

![Diagram of Native Windows Models ](assets/windows-AI-Foundry​.png)

## Choose between cloud-based and local AI services
You can integrate AI into your Windows application using either a local model or a cloud-based model. Consider the following aspects:

- Resource Availability
    - Local device: Running a model depends on the device’s CPU, GPU, NPU, memory, and storage. Small Language Models (SLMs), such as Phi, are suitable for local use. Copilot+ PCs include built-in models managed by Windows Copilot Runtime.
    - Cloud: Cloud platforms, such as Azure AI Services, provide scalable resources. Large Language Models (LLMs), such as OpenAI language models, require more resources and are available through the cloud.
- Data Privacy and Security
    - Local device: Data remains on the device, which can benefit privacy and security.
    - Cloud: Data is transferred to the cloud, which may raise privacy concerns.
- Accessibility and Collaboration
    - Local device: The model and data are accessible only on the device unless shared manually.
    - Cloud: The model and data can be accessed from anywhere with internet connectivity.
- Cost
    - Local device: There is no additional cost beyond the device hardware.
    - Cloud: Costs are based on usage and resources consumed.
- Maintenance and Updates
    - Local device: The user is responsible for maintenance and updates.
    - Cloud: The cloud provider manages maintenance and updates.

### Local AI services

**Use a custom model on your local machine**: You can train your own model using platforms like TensorFlow or PyTorch. You can integrate this custom model into your Windows application by running it locally with ONNX Runtime and AI Toolkit for Visual Studio Code.

**Use Windows Copilot Runtime**: When a local AI model is appropriate, you can use Windows Copilot Runtime features on Copilot+ PCs. Available features include:

- **Phi Silica**: a local, ready-to-use language model.
- **Recall**: a UserActivity API that uses AI to help search through past activity, supported by Click to Do, which uses Phi Silica to connect actions to content found by Recall.
- **AI Imaging**: generate various types of text descriptions for an image (Image Description), for scaling and sharpening images (Image Super Resolution) and identifying objects in images (Image Segmentation).
- **Windows Studio Effects**: for applying AI effects to the device camera or microphone.

Windows Copilot Runtime provides these AI-powered features through APIs. The models run locally and continuously in the background on Copilot+ PCs. These APIs are included in the Windows App SDK and are currently available in the latest experimental channel release of the Windows App SDK.




Next [Developer Setup](../7-image-description.md)