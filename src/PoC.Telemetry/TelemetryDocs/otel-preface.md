# Observability & OpenTelemetry

---

## Why Observability?

In a monolith, debugging is straightforward — one process, one log file, one debugger. In a distributed system (microservices, message queues, multiple databases), a single user action can touch **5+ services**. When something breaks or slows down, the question isn't *"what failed?"* — it's *"where in the chain did it fail, and why?"*

**Observability** is the ability to understand your system's internal state by examining its outputs — without deploying new code or attaching a debugger. It answers three questions:

1. **What happened?** → Logs
2. **How is it performing?** → Metrics
3. **Why is it broken / slow?** → Traces

> **Monitoring** tells you *when* things are broken. **Observability** tells you *why* — even for failure modes you didn't predict.

---

## What is Distributed Tracing?

When a user clicks "Withdraw €500", that request flows through:

```
Browser → PAM API → Database → Service Bus → FPCC Consumer → FraudForce → Database → PAM Callback
```

**Distributed tracing** follows that request across every service boundary, every queue, every database call — and stitches it into a single, end-to-end timeline. Each step is a **span**, and the full journey is a **trace**.

Without it, you have logs from 5 services with no way to connect them. With it, you see:

- *This* request took 3.2 seconds
- 2.8 seconds were spent in FraudForce
- The DB write took 12ms
- The Service Bus publish took 45ms

It turns "the system is slow" into "FraudForce is slow for high-value withdrawals."

---

## What is OpenTelemetry (OTel)?

OpenTelemetry is a **vendor-neutral, open-source observability framework** — the CNCF's second most active project after Kubernetes. It provides a single set of APIs, SDKs, and tools to **instrument, generate, collect, and export** telemetry data (traces, metrics, logs).

**Why it matters:** Before OTel, you were locked into a vendor's SDK (Datadog, New Relic, Application Insights). Switch vendor? Rewrite all instrumentation. OTel decouples instrumentation from the backend — instrument once, export anywhere.

---

## The Three Pillars of Telemetry

### 1. Traces & Spans

- A **trace** represents the entire journey of a request through your distributed system — from the user clicking "Withdraw" to the database write and the message published to Service Bus.
- A trace is composed of **spans**. Each span represents a single unit of work: an HTTP call, a DB query, a message publish.
- Spans have: **name, start/end time, attributes (key-value pairs), status, parent span ID**.
- Spans form a **tree** — the root span is the entry point, child spans are downstream operations.
- **Context propagation** (W3C `traceparent` header) is what stitches spans across service boundaries. Without it, you just have isolated logs.

```
[PAM: POST /withdrawals]  ──►  [FPCC: ProcessWithdrawal]  ──►  [DB: INSERT]
       root span                     child span                  child span
```

### 2. Metrics

Metrics are **numerical measurements collected over time** that describe the behavior of your system. They're the cheapest form of telemetry — pre-aggregated at the source, tiny to store, and ideal for dashboards and alerting.

Three instrument types:

- **Counter** — only goes **up**. Use for things you count.
  ```csharp
  var withdrawalCounter = meter.CreateCounter<long>("withdrawals.total");
  withdrawalCounter.Add(1, new("status", "success"));
  ```
  *"We processed 14,302 withdrawals today."*

- **Histogram** — records a **distribution of values**. Use for durations, sizes, latencies.
  ```csharp
  var durationHistogram = meter.CreateHistogram<double>("withdrawal.duration_ms");
  durationHistogram.Record(42.5, new("method", "card"));
  ```
  *"P50 withdrawal latency is 45ms, P99 is 320ms."*

- **Gauge (ObservableGauge)** — a **point-in-time snapshot** that can go up or down. Use for current state.
  ```csharp
  meter.CreateObservableGauge("active_connections", () => connectionPool.Count);
  ```
  *"Right now there are 17 active DB connections."*

How it flows in OTel:

```
Instrument  →  Meter  →  MeterProvider  →  Exporter (Prometheus / OTLP)
```

**Metrics vs. Traces — when to use what:**

| Question                                 | Use                      |
| ---------------------------------------- | ------------------------ |
| "How many errors per minute?"            | **Metrics** (Counter)    |
| "What's the P99 latency?"               | **Metrics** (Histogram)  |
| "Why was *this specific request* slow?"  | **Traces**               |
| "What called what, in what order?"       | **Traces**               |

> **Rule of thumb:** Metrics tell you *something is wrong*. Traces tell you *why*.

### 3. Logs (Structured)

- OTel is converging logs into the same pipeline. Structured logs carry `TraceId` and `SpanId`, so you can **correlate a log line to the exact span** that produced it.
- In .NET, `ILogger` + OTel exporter gives you this for free.

---

## Semantic Conventions

OTel defines **standard attribute names** so everyone speaks the same language:

- `http.request.method`, `http.response.status_code`, `db.system`, `messaging.system`
- This means dashboards, alerts, and queries work across services, teams, and even organizations without translation.

---

## Auto vs. Manual Instrumentation

OTel has two layers — and understanding the split is key:

- **Auto-instrumentation** — plug in a library, get telemetry for free. In .NET, one line each gives you spans for every HTTP request, every DB query, every outgoing HTTP call:
  ```csharp
  .AddAspNetCoreInstrumentation()   // incoming HTTP
  .AddHttpClientInstrumentation()   // outgoing HTTP
  .AddEntityFrameworkCoreInstrumentation() // DB queries
  ```
  This is your **80% for free** — no code changes, immediate visibility.

- **Manual instrumentation** — for your **business logic**. OTel can't know that a span should be called "Get FraudForce" or that `withdrawal.amount` is a meaningful attribute. You add these yourself:
  ```csharp
  using var activity = source.StartActivity("Get FraudForce");
  activity?.SetTag("fpcc.fraudforce.score", score);
  ```

> **The mental model:** Auto-instrumentation shows you the *plumbing* (HTTP, DB, messaging). Manual instrumentation shows you the *business flow*.

---

## OTel Collector

The **Collector** is a vendor-agnostic proxy that sits between your apps and your observability backend.

```
[App 1] ──┐
[App 2] ──┼──► [OTel Collector] ──► Jaeger / Prometheus / Azure Monitor / Grafana
[App 3] ──┘
```

Three pipeline stages:

1. **Receivers** — accept data (OTLP, Jaeger, Zipkin, Prometheus scrape)
2. **Processors** — transform, batch, filter, sample, redact PII
3. **Exporters** — send to backends (OTLP, Jaeger, Prometheus, vendor-specific)

**Why not export directly from the app?** The Collector gives you:

- **Decoupling** — change backend without redeploying apps
- **Buffering & retry** — apps don't block if the backend is down
- **Processing** — sampling, PII masking, enrichment — all in one place
- **Fan-out** — send to multiple backends simultaneously

---

## Weaver (OTel Weaver)

**Weaver** is a relatively new OTel tool for **managing semantic conventions as code**. You define your telemetry schema in YAML (metrics, spans, attributes), and Weaver can:

- **Validate** your definitions against OTel semantic conventions
- **Generate** documentation, code, and registry files from those YAML definitions
- **Enforce consistency** — your metric names, attribute keys, and span names are defined once, generated everywhere

Think of it as **OpenAPI/Swagger but for telemetry**. Instead of inventing metric names ad hoc, you declare them in a registry, and Weaver ensures they're correct and consistent.

---
