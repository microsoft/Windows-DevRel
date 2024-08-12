import torch
import torch.nn as nn
import torch.optim as optim
import torch.onnx
import sys

# Define a simple neural network model
class DummyModel(nn.Module):
    def __init__(self):
        super(DummyModel, self).__init__()
        self.fc1 = nn.Linear(10, 5)
        self.fc2 = nn.Linear(5, 2)

    def forward(self, x):
        x = torch.relu(self.fc1(x))
        x = self.fc2(x)
        return x

def create_dummy_model():
    model = DummyModel()
    
    # Dummy data
    dummy_input = torch.randn(10)
    target = torch.tensor([1, 0], dtype=torch.float32)
    
    # Define loss function and optimizer
    criterion = nn.MSELoss()
    optimizer = optim.SGD(model.parameters(), lr=0.001)

    # Train the model with dummy data
    model.train()
    for _ in range(10):  # Train for 10 iterations
        optimizer.zero_grad()
        output = model(dummy_input)
        loss = criterion(output, target)
        loss.backward()
        optimizer.step()

    return model, dummy_input

def convert_to_onnx(model, dummy_input, onnx_file_path):
    model.eval()
    torch.onnx.export(
        model,
        dummy_input,  # The model's input
        onnx_file_path,
        export_params=True,  # Store the trained parameter weights inside the model file
        opset_version=10,  # the ONNX version to export the model to
        do_constant_folding=True,  # whether to execute constant folding for optimization
        input_names=['input'],  # the model's input names
        output_names=['output'],  # the model's output names
        dynamic_axes={'input': {0: 'batch_size'}, 'output': {0: 'batch_size'}}  # variable length axes
    )
    print(f"Model has been converted to {onnx_file_path}")

def main():
    try:
        model, dummy_input = create_dummy_model()
        convert_to_onnx(model, dummy_input, ".temp/dummy_model.onnx")
        sys.exit(0)
    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()