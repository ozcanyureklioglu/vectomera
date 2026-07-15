# ☀️ Helios — AI-Powered Inventory & Product Intelligence Platform

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/PostgreSQL-pgvector-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" />
  <img src="https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white" />
  <img src="https://img.shields.io/badge/Ollama-LLM-000000?style=for-the-badge&logo=ollama&logoColor=white" />
  <img src="https://img.shields.io/badge/Semantic%20Kernel-AI-742774?style=for-the-badge" />
</p>

**Helios** is an AI-powered e-commerce and inventory management platform that unifies product catalogs, warehouse inventory management, and product reviews. Powered by a robust **RAG (Retrieval-Augmented Generation)** infrastructure, it enables **semantic search** and **intelligent data analysis** across all domain entities.

---

## 🏗️ Architecture Overview

The project is built upon **Clean Architecture** principles and orchestrated using **.NET Aspire**.

```
Helios.sln
├── Helios.AppHost          → .NET Aspire Orchestrator (Api + Worker)
├── Helios.Api              → REST API (Minimal API Endpoints)
├── Helios.Application      → Business rules, Interfaces, DTOs, Validations
├── Helios.Domain           → Entity models, BaseEntity
├── Helios.Infrastructure   → EF Core, Services, Ollama/SK Integrations
├── Helios.Worker           → Background consumers (MassTransit)
├── Helios.ServiceDefaults  → Shared Aspire configurations
└── docker-compose.yml      → PostgreSQL (pgvector) + RabbitMQ
```

### Layer Dependency Flow

```mermaid
graph TD
    API["Helios.Api<br/><i>Minimal API Endpoints</i>"] --> APP["Helios.Application<br/><i>Interfaces, DTOs, Validators</i>"]
    WORKER["Helios.Worker<br/><i>MassTransit Consumers</i>"] --> APP
    APP --> DOMAIN["Helios.Domain<br/><i>Entities</i>"]
    INFRA["Helios.Infrastructure<br/><i>EF Core, Services, AI</i>"] --> APP
    INFRA --> DOMAIN
    API --> INFRA
    WORKER --> INFRA
```

---

## 🤖 RAG (Retrieval-Augmented Generation) Pipeline

The core of Helios' AI capabilities lies within its **RAG** architecture. This flow semantically matches user queries with the most relevant database records, passing them to the LLM as context to generate highly accurate, context-aware, and hallucination-free responses.

```mermaid
sequenceDiagram
    participant User as 👤 User
    participant API as Helios.Api
    participant AI as AiService
    participant EMB as EmbeddingService
    participant PG as PostgreSQL + pgvector
    participant LLM as Ollama (Gemma3)

    User->>API: POST /ai/advice { query }
    API->>AI: GetAdviceAsync(query)
    
    Note over AI,EMB: 1️⃣ Embedding
    AI->>EMB: Query → Vector (nomic-embed-text)
    EMB-->>AI: float[] queryVector

    Note over AI,PG: 2️⃣ Retrieval (Vector Search)
    AI->>PG: L2Distance search (Top-3 × 3 tables)
    PG-->>AI: Most relevant chunk texts

    Note over AI,LLM: 3️⃣ Generation
    AI->>LLM: System Prompt + Context + Query
    LLM-->>AI: Synthesized Response
    
    AI-->>API: AiAdviceResponse
    API-->>User: 200 OK { answer }
```

### Vector Search Tables

The query vector is searched across the following **3 distinct VectorChunk** tables using `L2Distance` (Euclidean distance), retrieving the top **3 nearest records** from each:

| Table | Source Data | Description |
|---|---|---|
| `ProductVectorChunks` | Product Descriptions | Semantic chunks based on product details |
| `WarehouseInventoryVectorChunks` | Inventory Descriptions | Semantic chunks based on stock and warehouse details |
| `ProductReviewVectorChunks` | Product Reviews | Semantic chunks based on user rating, title, and comments |

---

## ⚙️ Embedding & Chunking Flow

Whenever new data is created (product, inventory, or review), **embedding** and **chunking** processes are automatically triggered in the background.

```mermaid
flowchart LR
    subgraph API ["Helios.Api"]
        A1[POST /products] --> P1[ProductService]
        A2[POST /warehouse-inventories] --> P2[WarehouseInventoryService]
        A3[POST /product-reviews] --> P3[ProductReviewService]
    end

    subgraph MQ ["RabbitMQ"]
        Q1([ProductEmbeddingEvent])
        Q2([CreateWarehouseInventoryEvent])
        Q3([CreateProductReviewEvent])
    end

    subgraph Worker ["Helios.Worker"]
        C1[ProductEmbeddingConsumer]
        C2[WarehouseInventoryEmbeddingConsumer]
        C3[ProductReviewEmbeddingConsumer]
    end

    subgraph Infra ["Infrastructure"]
        SK[SemanticKernelEmbeddingService<br/>TextChunker + nomic-embed-text]
        CS[ChunkService<br/>VectorChunk CRUD]
    end

    subgraph DB ["PostgreSQL + pgvector"]
        T1[(ProductVectorChunks)]
        T2[(WarehouseInventoryVectorChunks)]
        T3[(ProductReviewVectorChunks)]
    end

    P1 -- Publish --> Q1
    P2 -- Publish --> Q2
    P3 -- Publish --> Q3

    Q1 --> C1
    Q2 --> C2
    Q3 --> C3

    C1 --> SK --> CS --> T1
    C2 --> SK
    C3 --> SK
    CS --> T2
    CS --> T3
```

### Process Details

1. **API** saves the data and publishes an event to the RabbitMQ queue using `MassTransit (Publish)`.
2. A Consumer in the **Worker** listens to this event.
3. **SemanticKernelEmbeddingService** splits the text into paragraphs (chunks) using `TextChunker`, and then converts each chunk into a vector array (`float[]`) using Ollama's `nomic-embed-text` model.
4. **ChunkService** saves the generated vector chunks (`VectorChunk`) into the corresponding `pgvector` columns in PostgreSQL.

> **Review Embedding Format:**
> Before chunking, product reviews are formatted as follows:
> `"Point: {Point}/5. Title: {Title}. Comment: {Description}"`

---

## 🗄️ Domain Model

```mermaid
erDiagram
    Product ||--o{ ProductVectorChunk : "vector chunks"
    Product ||--o{ WarehouseInventory : "inventories"
    Product ||--o{ ProductReview : "reviews"
    Product }o--|| Brand : "brand"
    Product }o--|| Category : "category"
    Product ||--o{ ProductProperty : "properties"

    Warehouse ||--o{ WarehouseInventory : "inventories"
    Warehouse ||--o{ ProductReview : "reviews"

    WarehouseInventory ||--o{ WarehouseInventoryVectorChunk : "vector chunks"
    ProductReview ||--o{ ProductReviewVectorChunk : "vector chunks"

    Product {
        guid Id PK
        string Name
        string Sku
        string Slug
        string Description
        guid BrandId FK
        guid CategoryId FK
    }

    Warehouse {
        guid Id PK
        string Name
        string CityName
        string DistrictName
        float Longitude
        float Latitude
    }

    WarehouseInventory {
        guid Id PK
        guid WarehouseId FK
        guid ProductId FK
        int AvailableStock
        int IncomingStock
        int OutgoingStock
        decimal Price
        string Description
    }

    ProductReview {
        guid Id PK
        guid ProductId FK
        guid WarehouseId FK
        string Title
        string Description
        int Point
    }

    ProductVectorChunk {
        guid Id PK
        guid ProductId FK
        string ChunkText
        vector Embedding
        int ChunkIndex
        int TokenCount
    }
```

---

## 🛣️ API Endpoints

### Product Management (`/products`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/products` | Creates a new product and publishes an embedding event |
| `PUT` | `/products/{id}` | Updates an existing product |
| `GET` | `/products` | Lists products |

### Warehouse Inventory Management (`/warehouse-inventories`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/warehouse-inventories` | **Bulk** creates inventory records (Supports Partial Success) |
| `GET` | `/warehouse-inventories` | Lists inventory information |

### Product Reviews (`/product-reviews`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/product-reviews` | **Bulk** creates product review records |

### AI Assistant (`/ai`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/ai/advice` | RAG-based semantic Q&A. Scans all VectorChunk tables |

> 📌 All endpoints are accessible and testable via the **Swagger UI**.

---

## 🧰 Tech Stack

| Technology | Purpose |
|---|---|
| **.NET 9** | Framework (API, Worker, AppHost) |
| **.NET Aspire** | Service orchestration and observability |
| **Minimal API** | Endpoint definitions |
| **Entity Framework Core 9** | ORM & database access |
| **PostgreSQL 16 + pgvector** | Relational data + vector storage |
| **RabbitMQ** | Message queue (event-driven architecture) |
| **MassTransit** | Messaging infrastructure |
| **Ollama** | Local LLM execution (Gemma3, nomic-embed-text) |
| **Semantic Kernel** | AI orchestration, TextChunker, Embedding services |
| **OllamaSharp** | Ollama API client |
| **Pgvector.EntityFrameworkCore** | Vector operations (L2Distance) via EF Core |
| **FluentValidation** | Request validation |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Ollama](https://ollama.com/)

### 1. Start Infrastructure Services

```bash
docker-compose up -d
```

This command will start the following services:

| Service | Port | Description |
|---|---|---|
| PostgreSQL (pgvector) | `5432` | Database (`heliosdb`) |
| RabbitMQ | `5672` / `15672` | Message queue / Management UI |

### 2. Download Ollama Models

```bash
ollama pull nomic-embed-text
ollama pull gemma3:4b
```

| Model | Size | Usage |
|---|---|---|
| `nomic-embed-text` | ~274 MB | Text embedding (vector generation) |
| `gemma3:4b` | ~3.3 GB | Chat / Q&A (LLM) |

### 3. Run the Application

```bash
# Using .NET Aspire (Starts Api + Worker together)
dotnet run --project Helios.AppHost

# Or run them separately
dotnet run --project Helios.Api
dotnet run --project Helios.Worker
```

### 4. Swagger UI

Once the application is running, you can access the Swagger interface at:

```
http://localhost:<port>/swagger
```

---

## 📁 Configuration

You can customize the following settings in the `appsettings.json` file:

```json
{
  "OllamaOptions": {
    "Endpoint": "http://localhost:11434",
    "EmbeddingModel": "nomic-embed-text",
    "ChatModel": "gemma3:4b"
  }
}
```

---

## 📄 License

This project is for private use.
