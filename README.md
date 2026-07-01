# MoodTracker

一個記錄每日心情的 RESTful API，使用者可以用五種表情符號快速記錄當下心情，並查看月份／年度的心情統計報表。本專案以學習 ASP.NET Core 後端開發為目的，逐步實作資料庫設計、JWT 身份驗證、CRUD 操作與統計功能。

## 功能特色

- **使用者系統**：註冊、登入，密碼驗證
- **JWT 身份驗證**：登入後取得 Token，所有心情紀錄 API 皆需驗證身份
- **心情紀錄 CRUD**：新增、查詢、修改、刪除心情紀錄（僅限本人）
- **彈性查詢**：依年／月／日篩選紀錄
- **心情統計**：統計指定年／月的五種心情分佈，找出最常出現的心情
- **資料安全**：使用者只能存取與修改自己的資料，未授權操作會回傳 403

## 技術棧

| 類別 | 技術 |
|------|------|
| 語言 / 框架 | C# / ASP.NET Core Web API |
| ORM | Entity Framework Core 8.0 |
| 資料庫 | Microsoft SQL Server |
| 身份驗證 | JWT (JSON Web Token) |
| API 文件 | Swagger / OpenAPI |

## API 端點

### 身份驗證 `/api/auth`

| Method | Endpoint | 說明 |
|--------|----------|------|
| POST | `/api/auth/register` | 註冊新帳號 |
| POST | `/api/auth/login` | 登入，取得 JWT Token |

### 心情紀錄 `/api/moods`（需要 JWT Token）

| Method | Endpoint | 說明 |
|--------|----------|------|
| GET | `/api/moods` | 查詢紀錄，支援 `year` / `month` / `day` 篩選 |
| GET | `/api/moods/{id}` | 查詢單筆紀錄 |
| GET | `/api/moods/stats` | 取得心情統計，支援 `year` / `month` 篩選 |
| POST | `/api/moods` | 新增一筆心情紀錄 |
| PUT | `/api/moods/{id}` | 修改紀錄（限本人） |
| DELETE | `/api/moods/{id}` | 刪除紀錄（限本人） |

## 資料模型

**User**
- Id, Username, Email, Password, CreatedAt

**MoodEntry**
- Id, UserId, MoodType（限定五種 emoji：😄 😊 😐 😟 😭）, Content, Tags, RecordDate, CreatedAt

## 開始使用

### 環境需求

- .NET 8 SDK
- SQL Server（本機或遠端）

### 安裝步驟

1. Clone 此專案
   ```bash
   git clone https://github.com/Silen237/MoodTracker.git
   cd MoodTracker
   ```

2. 設定資料庫連線
   複製 `appsettings.Example.json` 為 `appsettings.json`，填入你的資料庫連線字串與 JWT 密鑰：
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "your-connection-string-here"
     },
     "JwtSettings": {
       "SecretKey": "your-secret-key-here",
       "Issuer": "MoodTracker",
       "Audience": "MoodTrackerUsers",
       "ExpiresInDays": 7
     }
   }
   ```

3. 執行資料庫遷移
   ```bash
   dotnet ef database update
   ```

4. 啟動專案
   ```bash
   dotnet run
   ```

5. 開啟 Swagger 測試 API
   ```
   https://localhost:7000/swagger
   ```

## API 使用範例

### 註冊
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "張三",
  "email": "test@example.com",
  "password": "123456"
}
```

### 登入
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "123456"
}
```

回應會附上 JWT Token，後續請求需在 Header 帶上：
```
Authorization: Bearer {your-token}
```

### 新增心情紀錄
```http
POST /api/moods
Authorization: Bearer {your-token}
Content-Type: application/json

{
  "moodType": "😄",
  "tags": "天氣",
  "content": "今天陽光普照，出門散步。"
}
```

## 專案結構

```
MoodTracker/
├── Controllers/
│   ├── AuthController.cs
│   └── MoodsController.cs
├── Data/
│   └── AppDbContext.cs
├── Models/
│   ├── User.cs
│   └── MoodEntry.cs
├── Migrations/
└── Program.cs
```


## 授權

本專案僅作為個人學習與作品集展示用途。
