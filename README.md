# Device Data API

## Overview
Simple C# backend for storing and querying medical device packets, with a Blazor WebAssembly frontend.  
Supports patient ID, source, timestamp, and measured parameters.


> Developed and tested in **Visual Studio 2026** with **.NET 10K**
---

## Features
- Add / get device packets via REST API
- Filter by patient, source, time range, parameter range
- Thread-safe in-memory storage
- Async API (ready for future DB or disk storage)
- Blazor WebAssembly UI to view and manage packets

---

## Running
1. Open the solution in **Visual Studio**  
2. Set **Backend** and **Frontend** as startup projects (Multiple startup projects)  
3. Run both in parallel (Start button)
The frontent should be visible in browser: `https://localhost:7273/device-data` 

> Tip: The backend exposes a REST API; the frontend calls it via HTTPS.

---

## Tests
- Unit tests cover validation
