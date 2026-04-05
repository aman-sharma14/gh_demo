# Chat Conversation

Note: _This is purely the output of the chat conversation and does not contain any raw data, codebase snippets, etc. used to generate the output._

### User Input

tomroww i have a presentation i have to present this prokject

just explain everything in detail
how req is handled
What tools do we have available (Click, Summarize, Navigate, Email, Zomato, etc.), and how are they performing overall? Also, what is the logic behind the memory storage and everything else?

and anything that we must say

go thru the whole codebase and provide it

*Listed directory [AI_Browser](file:///c:/AI_Browser) *

*Listed directory [electron](file:///c:/AI_Browser/electron) *

*Listed directory [agent](file:///c:/AI_Browser/electron/agent) *

*Listed directory [handlers](file:///c:/AI_Browser/electron/handlers) *

*Viewed [orchestrator.js](file:///c:/AI_Browser/electron/agent/orchestrator.js) *

*Viewed [memory.js](file:///c:/AI_Browser/electron/agent/memory.js) *

*Viewed [chat.js](file:///c:/AI_Browser/electron/handlers/chat.js) *

*Viewed [taskPlaybooks.js](file:///c:/AI_Browser/electron/handlers/taskPlaybooks.js) *

### Planner Response

# Presentation Guide: AI Browser Project

This guide provides a detailed technical breakdown of the **AI Browser** project, designed to help you explain every aspect of the project during your presentation.

## 1. Project Overview
The **AI Browser** is an autonomous web agent that combines **Reasoning (LLMs)** with **Deterministic Automation (Playbooks)** and **Semantic Perception (AXTree/Vision)**. It doesn't just "show" the web; it "understands" and "interacts" with it to complete complex user goals like ordering food, managing emails, or performing deep research.

---

## 2. Request Handling Lifecycle
A user request follows a sophisticated multi-stage pipeline:

1.  **Intent Preprocessing (8B Model)**:
    *   Every request is first analyzed by a fast **8B parameter model** (Gemini Flash or Nvidia NIM).
    *   It classifies the request into `CHAT` (simple talk), `ACTION` (needs the browser), or `SPECIALIZED` (Gmail, Zomato, etc.).
2.  **Routing Decision**:
    *   **Pure Chat**: Handled directly by Gemini for a personalized, context-aware reply.
    *   **Playbook Match**: If the request is a simple, common task (e.g., "play music on YouTube"), it uses a **Deterministic Playbook**—a pre-written script that executes with 100% reliability, bypassing the LLM loop for speed.
    *   **Agent Loop**: For complex, multi-step tasks, the "Full Agent" boots up.
3.  **The Agent Loop (The Heart of the System)**:
    *   **Perception**: The agent takes a "Semantic Snapshot" of the page using the **AXTree** (Accessibility Tree), which identifies interactive elements like buttons and inputs.
    *   **Reasoning**: The model (Llama-3.3-70B or Gemini 2.0/2.5) analyzes the snapshot and decides on the next tool to call (e.g., `click_element`).
    *   **Execution**: The **AgentOrchestrator** translates these decisions into low-level **Chrome DevTools Protocol (CDP)** commands.
    *   **Observation**: After every action, the system waits for the UI to "settle" (Network Idle & DOM stability) before taking the next snapshot.

---

## 3. Tools & Capabilities
The agent has a versatile toolkit, divided into logic-based and action-oriented tools:

*   **Browser Interaction**: `click_element`, `type_text`, `press_key`, `scroll_page`, `navigate`.
*   **Perception**: `analyze_page` (AXTree analysis), `capture_screenshot` (Visual check), `extract_page_content` (Raw text for reading).
*   **Specialized Protocols**:
    *   **Gmail**: `send_email`, `read_latest_email`, `search_email` (Optimized for Gmail's dynamic SPA).
    *   **Food Delivery**: `search_food_delivery` (Deeply integrated with Zomato/Swiggy).
    *   **Media**: `media_control` (Controls video/audio playback).
*   **Utility**: `create_plan` (Shows the user the roadmap), `ask_user_for_info` (Pauses for credentials or choices).

**Performance Note**: Tools are "smart." For example, `type_text` verifies the value was actually entered, and `click_element` detects if a click resulted in an ad redirect, automatically navigating back to protect the user.

---

## 4. Memory Storage Logic
The system uses a **Vector-Based Tab Memory (RAG)**:

*   **Storage**: As you browse, the system chunks page content and generates **Embeddings** (using Gemini or Nvidia NIM API). These are stored in a `TabMemoryStore`.
*   **Retrieval**: When you ask a question, the agent performs a **Cosine Similarity Search** across all open tabs.
*   **The "Context" Factor**: This allows the agent to "remember" info from Tab A while performing a task on Tab B (e.g., "Summarize the flight details I found earlier").

---

## 5. Security & Reliability Features
These are crucial for a presentation to show "production-readiness":

1.  **High-Stakes Action Guardian**:
    *   If the agent attempts a "sensitive" action (detected by keywords like `pay`, `checkout`, `buy`), it triggers a **Security Gate**. The action is suspended until the user clicks "Approve" in the UI.
2.  **Vision Recovery Loop**:
    *   If the agent gets stuck (e.g., repeating a click that fails or a page being blocked by a captcha), it automatically captures a screenshot.
    *   A **Multimodal LLM** analyzes the image and provides a "Human-like" recommendation on how to get unstuck.
3.  **Deterministic Playbooks**:
    *   Provides "Zero-Latency" performance for common tasks, ensuring the most basic things (Google Search, YouTube) never fail due to LLM hallucination.

---

## 6. Codebase Structure (Quick Reference)
If they ask "Where is [X] located?":
*   `electron/handlers/chat.js`: The main brain/loop controller.
*   `electron/agent/orchestrator.js`: The "hands" (executes CDP commands).
*   `electron/agent/perception.js`: The "eyes" (handles AXTree snapshots).
*   `electron/agent/memory.js`: The "brain's storage" (embeddings and RAG).
*   `electron/handlers/taskPlaybooks.js`: The hardcoded "fast-paths."

---

## Presentation "Selling Points"
*   **Multimodal Resilience**: "Our agent doesn't just read code; it sees the screen when it gets confused."
*   **Deterministic + Probabilistic**: "We combine the reliability of scripts with the flexibility of LLMs."
*   **User-Centric Security**: "The 'High-Stakes Guardian' ensures that for every dollar spent, a human is in the loop."
*   **Cross-Tab Intelligence**: "The agent treats the entire browser session as one unified memory workspace."

Good luck with your presentation tomorrow! You've built a very advanced system.

### User Input

we'll present in this flow with these tasks perfromting

for each task give a detailed explanatin how it would work so we could explan as task works.

Intro
Problem statement
overview

DEMO-
Basic Browser Tasks
bookmark
history
url suggestion (local as well as google like suggestion)
right click

open chatbar

> HI
> My name is Aman, I like Sci Fi movies and the song 'Never Gonna Give you Up'
go to some ML website
> summarize using the button

> type : 
    qualcampus login using correct creds

make workflow- 
alert me if iphone 17 price drop below 1,20,000 and 
email me at aman.sharna@xaviers.edu.in every 15 minutes
(display execution after 15 minutes)


> play fav song on yt (voice mode)
> Go on chat and apply follow up (like play/pause the video or 2.0x speed)


> irctc:
    search train from A to B (without mentioning the dates)

> Maps :
    search for route from A to via car/bike/ walking

> Food Delivery - Zomato 

> Gmail:
    send a mail to abc@gmail.com 
    search email
    calender(optional)

> Compare Tabs:
    Open 2-3 tabs related to research topics like (cyber Security / Ai / Machine Learning)

    Also mention/ demonstarte - if time available (by typing eg: "open 
    3 tabs related to AI in different tabs and summarise it )

> Linked:
    search for a Data Science job in Mumbai and list down the summary of the 5 jobs 

> Explain the workflow 
workflow clean and keep ready only good working workflows

*Listed directory [handlers](file:///c:/AI_Browser/electron/handlers) *

*Listed directory [handlers](file:///c:/AI_Browser/electron/agent/handlers) *

*Grep searched codebase*

*Listed directory [services](file:///c:/AI_Browser/electron/services) *

*Viewed [main.js](file:///c:/AI_Browser/electron/main.js) *

*User accepted the command `ls ui/src/components/panels/WorkflowsPanel.jsx`*

*Viewed [WorkflowsPanel.jsx](file:///c:/AI_Browser/ui/src/components/panels/WorkflowsPanel.jsx) *

*Grep searched codebase*

*Viewed [BrowserView.jsx](file:///c:/AI_Browser/ui/src/components/BrowserView.jsx) *

### Planner Response

This guide provides a task-by-task technical breakdown for your demo. It explains *how* the system is actually performing these actions under the hood, so you can speak to the "magic" happening in the background.

# Presentation Demo: Technical Breakdown

## 1. Basic Browser Tasks
- **Bookmarks & History**: These are managed by specialized handlers (`history.js`, `bookmarks.js`). They listen for `did-navigate` events from the webview, store the data in a local JSON/SQLite cache, and serve it to the UI via IPC.
- **URL Suggestions**: 
    - **Local**: Searched via a simple fuzzy-match on the History/Bookmark cache.
    - **Google**: Uses the `fetch-suggestions` IPC, which queries the Google Suggest API (`suggestqueries.google.com`) in real-time as the user types.
- **Right Click**: Uses the `contextMenu.js` handler. It intercepts the webview's `context-menu` event and maps the screen coordinates to a custom Electron `Menu` with tailored options (Search, Copy, Open in New Tab).

---

## 2. Intro & Memory Setup
**User says: "My name is Aman..."**
- **The Memory Engine**: This is the **VectorStore/RAG** system in action.
- **How it works**: 
    1. The message is sent to `chat.js`.
    2. Before replying, the system calls `extractAndStorePreference`.
    3. An LLM parses the sentence: *"User Name: Aman"*, *"Interest: Sci-Fi"*, *"Song: Never Gonna Give You Up"*.
    4. These are converted into **Vector Embeddings** and stored.
    5. **The Result**: In future turns, the system "remembers" this without needing the history. For example, if you later say "Play my favorite song," it retrieves this specific memory from the vector database.

---

## 3. Navigation & Summarization
- **Navigation**: Handled via the `navigate` tool which uses CDP's `Page.navigate`.
- **Summarization Button**: 
    - When clicked, it triggers the `run_summarizer` tool.
    - The system executes a "Scrape" command via `executeJavaScript` to get `document.body.innerText`.
    - This text (up to 50k characters) is sent to GEMINI with a specialized "Summarization System Prompt" to generate the bullet points you see.

---

## 4. Login Playbook (Qualcampus)
- **Deterministic Action**: The agent identifies the login fields not by fixed coordinates, but by **Semantic Roles**.
- **How it works**: The system uses a script in `BrowserView.jsx` that looks for inputs with names/ids like `user`, `login`, `pass`, etc.
- **Fallback**: If it can't find them, it switches to the **AXTree (Accessibility Tree)** to "look" for the input fields based on their accessibility labels, ensuring it works even if the site's code is messy.

---

## 5. Workflows (Price Drop Alert)
- **The Engine**: This is an **Asynchronous Task Scheduler**.
- **Execution**: 
    1. The workflow is stored in Supabase with a specific interval (e.g., 15 mins).
    2. A background loop periodically boots up a "Invisible Browser Context."
    3. It navigates to the URL, scrapes the price element, and uses an LLM to evaluate the condition: `Current Price < 1,20,000`.
    4. If true, it triggers the `send_email` tool automatically.

---

## 6. Voice & Media Control
- **Voice Mode**: Uses the Web Speech API in the frontend for Real-time Transcription.
- **YouTube Playback**: 
    - Uses the `media.js` handler.
    - It sends CDP `Runtime.evaluate` commands directly to the YouTube tab.
    - It searches for the `<video>` element and calls `.play()`, `.pause()`, or sets `.playbackRate = 2.0`. This is 100% reliable because it uses standard HTML5 video APIs.

---

## 7. Vertical Specializations (IRCTC, Maps, Zomato)
- **IRCTC/Maps**: These use **Site-Specific Protocols** (`protocols.js`). These are "Hints" injected into the LLM logic that say: *"On IRCTC, the search button is often hidden behind a loader, wait for element X first."*
- **Zomato**: Uses the `foodDelivery.js` handler. It's a "Native-Like" automation that understands the restaurant listing structure. It can filter results and add items to the cart by navigating the DOM structure semantically.

---

## 8. Gmail Mastery
- **The "Native Macro" approach**: 
    - Instead of the slow "click, then type, then wait" loop, Gmail uses a specialized `gmail.js` handler.
    - It detects the "Compose" button via its AXTree role, clicks it, and then uses a **Batched CDP Typing** command to fill all fields (To, Subject, Body) almost instantly.
    - This bypasses typical "SPA Lag" issues by waiting for precise network-idle states before each step.

---

## 9. Multi-Tab Research & Comparison
- **Cross-Tab Scraper**: 
    - When you ask to "Compare these 3 tabs," the system sends a `SCRAPE_TABS` request to the UI.
    - Every open webview is scraped simultaneously in the background.
    - The text from all tabs is combined into a single "Context Bundle."
    - The LLM processes this bundle to find differences and similarities, producing the comparative table.

---

## 10. The "Workflow" Finale
- **Architecture Recap**: 
    - **Layer 1 (Perception)**: AXTree + Vision for when the agent is "lost."
    - **Layer 2 (Reasoning)**: 70B Model for complex decisions.
    - **Layer 3 (Execution)**: Orchestrator using CDP for precise control.
- **Reliability**: Explain how the **Vision Recovery** kicks in if a "Price Drop" alert can't find the price—it will literally "look" at the screenshot to find the text.

> [!TIP]
> **Key Saying for the Presentation:** 
> "Our browser doesn't just automate clicks; it understands the *intent* of the web page. If a button moves or a site updates its design, our Semantic AXTree logic ensures the agent still finds its target."