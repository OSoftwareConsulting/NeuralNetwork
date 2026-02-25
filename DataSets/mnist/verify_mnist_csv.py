import pandas as pd
import numpy as np

def verify_mnist_csv(csv_filename):
    print(f"Loading {csv_filename} into Pandas... (This might take a moment)")
    
    # Load the CSV
    df = pd.read_csv(csv_filename, header=None)
    
    print(f"\n--- Data Verification for {csv_filename} ---")
    print(f"Total Dataset Shape: {df.shape}") 
    
    # Separate the pixels (first 784 columns) and labels (last 10 columns)
    X = df.iloc[:, :784].values
    y = df.iloc[:, 784:].values
    
    print(f"Pixel Data Shape:    {X.shape}")
    print(f"Label Data Shape:    {y.shape}")
    
    print("\n--- Verifying All Labels ---")
    
    # 1. Check if all values in the label columns are strictly 0 or 1
    is_binary = np.all(np.isin(y, [0, 1]))
    
    # 2. Check if every single row sums to exactly 1
    row_sums = np.sum(y, axis=1)
    is_one_hot = np.all(row_sums == 1)
    
    if is_binary and is_one_hot:
        print("✅ SUCCESS: All labels are perfectly one-hot encoded!")
        print("   (Every row contains exactly nine 0s and one 1)")
    else:
        print("❌ ERROR: There is a problem with the label formatting.")
        if not is_binary:
            print("   - Found values other than 0 and 1 in the label columns.")
        if not is_one_hot:
            bad_rows_count = np.sum(row_sums != 1)
            print(f"   - Found {bad_rows_count} rows where the labels do not sum to 1.")
            
    # 3. Show the distribution of the digits
    print("\n--- Label Distribution ---")
    # Convert one-hot back to standard digits (0-9) for all rows at once
    actual_digits = np.argmax(y, axis=1)
    
    # Count how many times each digit appears
    unique_digits, counts = np.unique(actual_digits, return_counts=True)
    
    for digit, count in zip(unique_digits, counts):
        print(f"Digit {digit}: {count:,} images")

# ==========================================
# Execution
# ==========================================
if __name__ == "__main__":
    # List both files you want to verify
    files_to_check = ['train_data.csv', 'test_data.csv']
    
    for filename in files_to_check:
        try:
            verify_mnist_csv(filename)
            print("\n" + "="*50 + "\n")
        except FileNotFoundError:
            print(f"⚠️ ERROR: Could not find '{filename}'. Make sure the file exists in this directory!")
            print("\n" + "="*50 + "\n")