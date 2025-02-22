# Azure Blob Chunked Upload - .NET Core 8

## 📦 About
This project demonstrates how to upload large files to Azure Blob Storage using **chunking * *and * *parallelism * * in **C# .NET Core 8**.

## 🚀 Features
- Chunking large files into smaller parts.
- Parallel uploads using `Parallel.ForEachAsync`.
-Retry logic with exponential backoff.
- Uses Azure Blob SDK (`Azure.Storage.Blobs`).

## 🧰 Prerequisites
- .NET Core 8 SDK
- Azure Blob Storage Account

## ⚡ Getting Started

1. **Clone the repo:**
```bash
git clone https://github.com/your-repo/AzureBlobChunkedUpload.git
cd AzureBlobChunkedUpload
```

2. **Install dependencies:**
```bash
dotnet add package Azure.Storage.Blobs
```

3. **Update `Program.cs` with your Azure Blob connection string and file paths.**

4. **Run the app:**
```bash
dotnet run
```

## 💡 How It Works
- The file is split into **8MB chunks**.
- Chunks are uploaded **in parallel** (4 at a time).
- After all chunks are uploaded, they are **committed** into a single blob.

## 📸 Screenshots
- **Azure Portal showing uploaded Blob**
- **Console logs showing chunk uploads**

## 📃 License
MIT
