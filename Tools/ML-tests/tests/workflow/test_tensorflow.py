import sys
sys.stdout.reconfigure(encoding='utf-8')
import os
os.environ['TF_ENABLE_ONEDNN_OPTS'] = '0'

import tensorflow as tf
import numpy as np



# Ensure TensorFlow is running on CPU
tf.config.set_visible_devices([], 'GPU')

# Generate dummy data
num_samples = 1000
num_features = 10
num_classes = 3

X_train = np.random.random((num_samples, num_features)).astype(np.float32)
y_train = np.random.randint(num_classes, size=(num_samples,))

# Convert the labels to one-hot encoding
y_train = tf.keras.utils.to_categorical(y_train, num_classes)

# Create a simple feedforward neural network model
model = tf.keras.Sequential([
    tf.keras.layers.InputLayer(shape=(num_features,)),
    tf.keras.layers.Dense(64, activation='relu'),
    tf.keras.layers.Dense(32, activation='relu'),
    tf.keras.layers.Dense(num_classes, activation='softmax')
])

# Compile the model
model.compile(optimizer='adam',
              loss='categorical_crossentropy',
              metrics=['accuracy'])

# Train the model
model.fit(X_train, y_train, epochs=10, batch_size=32, validation_split=0.2, verbose=0)

# Evaluate the model
loss, accuracy = model.evaluate(X_train, y_train)
print(f"Loss: {loss:.4f}, Accuracy: {accuracy:.4f}")

# Save the model
model.save('.temp/dummy_model.keras')

# Load the model (for demonstration purposes)
loaded_model = tf.keras.models.load_model('.temp/dummy_model.keras')

# Verify the loaded model
loss, accuracy = loaded_model.evaluate(X_train, y_train, verbose=0)
print(f"Loaded Model - Loss: {loss:.4f}, Accuracy: {accuracy:.4f}")