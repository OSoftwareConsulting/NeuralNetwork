import struct
import numpy as np

def load_mnist_images(filename):
    """
    Parses an IDX3 file containing MNIST images and flattens them.
    """
    with open(filename, 'rb') as f:
        magic, num_images, rows, cols = struct.unpack(">IIII", f.read(16))
        
        # Read the pixel data
        image_data = np.frombuffer(f.read(), dtype=np.uint8)
        
        # Reshape into a 2D array: (number of images, 784 flattened pixels)
        images = image_data.reshape(num_images, rows * cols)
        
    return images

def load_mnist_labels(filename):
    """
    Parses an IDX1 file containing MNIST labels.
    """
    with open(filename, 'rb') as f:
        magic, num_items = struct.unpack(">II", f.read(8))
        labels = np.frombuffer(f.read(), dtype=np.uint8)
        
    return labels

def save_to_csv(images, labels, output_filename):
    """
    One-hot encodes labels, merges with image data, and saves to CSV.
    """
    print(f"Processing data for {output_filename}...")
    
    # 1. One-hot encode the labels
    # np.eye(10) creates a 10x10 identity matrix (diagonal 1s, rest 0s).
    # Indexing it with our labels array instantly grabs the correct row of 0s and 1s!
    one_hot_labels = np.eye(10, dtype=np.uint8)[labels]
    
    # 2. Combine the flattened images (784 columns) and one-hot labels (10 columns)
    # np.hstack concatenates them horizontally, resulting in 794 columns.
    combined_data = np.hstack((images, one_hot_labels))
    
    # 3. Save to CSV
    # fmt="%d" ensures the data is saved as integers rather than scientific notation
    print(f"Saving to {output_filename} (This may take a minute)...")
    np.savetxt(output_filename, combined_data, delimiter=",", fmt="%d")
    print(f"Successfully saved {output_filename}!\n")

# ==========================================
# Execution
# ==========================================
if __name__ == "__main__":
    # Load Training Data
    X_train = load_mnist_images('train-images.idx3-ubyte')
    y_train = load_mnist_labels('train-labels.idx1-ubyte')
    
    # Load Test Data
    X_test = load_mnist_images('t10k-images.idx3-ubyte')
    y_test = load_mnist_labels('t10k-labels.idx1-ubyte')
    
    # Save to CSV files
    save_to_csv(X_train, y_train, 'train_data.csv')
    save_to_csv(X_test, y_test, 'test_data.csv')