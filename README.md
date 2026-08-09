# Spectra Stream

**Quest storefront and live overlay for Ko-fi-powered streams.** Viewers buy
preset challenges through Ko-fi; quests appear live on stream; the streamer
clears them as they're completed.

## Project Goals
- Build a production-ready quest system in **C#/.NET** (current)
- Translate to **Go** to demonstrate cross-language architecture skills
- Showcase clean, config-driven design for maintainable, scalable systems

## How It Works
1. **Store** — viewers browse preset quests and packages, copy a quest token
   (e.g. `#ss2wk`), and are directed to the streamer's Ko-fi page
2. **Ko-fi** — the viewer pays and pastes the token into their Ko-fi message.
   This app handles no money
3. **Webhook** — Ko-fi POSTs the payment event to `/api/kofi/webhook`; the
   app verifies, dedupes, matches the token, and enqueues the quest
4. **Overlay** — the quest queue displays live on stream (OBS browser source)
   via SignalR
5. **Admin** — the streamer removes quests from a private panel as they're
   completed

## Current Status
- **C# backend**: catalog, live queue, Ko-fi webhook intake complete
- **Frontend**: store/overlay/admin pages being converted to the new flow
- **Go translation**: follows after C# v1 ships

## Architecture
- **Catalog** (read-only, fail-fast JSON load): reusable objectives referenced
  by preset quests — no magic strings
- **Queue** (in-memory, lock-guarded): live quest instances, removed whole
- **Webhook** (composition point): verify → dedupe → match → enqueue →
  broadcast; returns 200 to anything genuinely from Ko-fi so retries stop
- **Transport-free services**: SignalR broadcasting lives in controllers only

## Configuration
The Ko-fi verification token is supplied via environment variable
(`Kofi__VerificationToken`) — never committed.

---

*The C# version is the stable baseline. The Go translation follows once v1
is test-confirmed.*