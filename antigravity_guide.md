# Antigravity — Complete Guide for This Project

## What is Antigravity?

Antigravity is an **AI coding assistant** (like Copilot, but agentic). It can:
- Read, write, and edit files
- Run terminal commands (with your approval)
- Browse the web and open a browser
- Remember your rules across every conversation
- Use custom **Skills** (specialized instruction packs)
- Follow **Rules** (always-on guidelines in `AGENTS.md`)

---

## The Big Picture — How Antigravity "Thinks"

```
Every conversation starts like this:
┌──────────────────────────────────────────────┐
│  1. Load Global Rules  (AGENTS.md - machine) │
│  2. Load Project Rules (AGENTS.md - project) │
│  3. Load Skills matching the current task    │
│  4. Read your request                        │
│  5. Plan → Execute → Verify                  │
└──────────────────────────────────────────────┘
```

---

## Complete Folder Structure

```
MACHINE LEVEL (applies to ALL your projects)
C:\Users\admin\.gemini\config\
├── AGENTS.md                  ← Global rules (always read)
└── skills\
    └── <skill-name>\
        ├── SKILL.md           ← Skill instructions
        ├── scripts\           ← Helper scripts
        ├── examples\          ← Code examples
        ├── resources\         ← Templates / assets
        └── references\        ← Extra docs

─────────────────────────────────────────────

PROJECT LEVEL (applies only to THIS project)
d:\Projects\Transaction-Sql-Crud-Operation\
├── .agents\
│   ├── AGENTS.md              ← ✅ Project rules (YOU CREATED THIS)
│   └── skills\
│       └── <skill-name>\
│           ├── SKILL.md       ← Project-specific skill
│           ├── scripts\
│           ├── examples\
│           └── resources\
│
├── .github\
│   ├── copilot-instructions.md   ← For GitHub Copilot ONLY
│   ├── CODING_GUIDELINES.md      ← For GitHub Copilot ONLY
│   └── CODING_GUIDELINES_API.md  ← For GitHub Copilot ONLY
│
├── Transaction Sql Crud Operation\  ← Main API project
├── CommonLogger\                     ← Shared logging
├── Models\                           ← Shared models
├── Qualification\
├── Real-Time-MCP.FunctionApp\
└── Transaction Sql Crud Operation.slnx
```

---

## The Three Customization Layers

### 1. AGENTS.md — Rules (Always On)

> **Location:** `.agents/AGENTS.md` in your project  
> **What it does:** Antigravity reads this at the start of EVERY conversation and follows every rule automatically. You never need to repeat yourself.

**Your current rules cover:**
- ✅ No hardcoded values
- ✅ Functions ≤ 40 lines
- ✅ Error logging to MongoDB
- ✅ API-First design (camelCase, no HTTP verbs in names)
- ✅ Security (parameterized queries, OWASP)
- ✅ `pageIndex` / `pageSize` on all list APIs
- ✅ React standards

---

### 2. Skills — Specialized Instruction Packs

> **Location:** `.agents/skills/<skill-name>/SKILL.md`  
> **What it does:** A skill is a reusable set of step-by-step instructions for a complex, recurring task. Antigravity **automatically detects** when to use a skill based on your request.

**Example skills you could create for this project:**
| Skill Name | Triggers When You Say... |
|---|---|
| `new-api-endpoint` | "add a new API", "create endpoint for..." |
| `error-logging-setup` | "add logging", "log this error" |
| `sql-crud-generator` | "create CRUD for table X" |
| `api-first-design` | "design an API", "plan new feature" |

---

### 3. Subagents (Advanced)

> **Location:** `.agents/agents/<agent-name>/`  
> A subagent is a specialized mini-agent Antigravity can spawn to do a specific job (e.g., "run all tests and report results"). This is advanced — start with Skills first.

---

## How to Develop a New Feature — Step by Step

### Step 1: Tell Antigravity What You Want

Just describe the feature in plain English:

> "I want to add a new API endpoint to get all transactions with pagination"

Antigravity will:
1. Read your `AGENTS.md` rules automatically
2. Plan the implementation (API-First)
3. Ask you to approve before making changes
4. Write the code following ALL your rules

---

### Step 2: Antigravity's Planning Mode

For any complex feature, Antigravity will:
1. **Research** — read your existing code to understand patterns
2. **Create a plan** — show you exactly what files will be created/modified
3. **Wait for approval** — you click "Proceed" or give feedback
4. **Execute** — make all changes
5. **Verify** — build/test to confirm it works

---

### Step 3: Review and Iterate

You can say things like:
- "The function is too long, split it"
- "Add error logging here"
- "Follow the API-First approach for this"

Antigravity will remember your AGENTS.md rules and apply them.

---

## Creating a Skill for New API Development

Here's how to create a skill that guides Antigravity every time you add a new API endpoint:

### File: `.agents/skills/new-api-endpoint/SKILL.md`

```markdown
---
name: new-api-endpoint
description: >
  Step-by-step guide for creating a new API endpoint in this project.
  Triggers when user says: "add API", "create endpoint", "new route", 
  "new controller action".
---

## Steps for Every New API Endpoint

1. **Check if the API already exists** — search the codebase first.
2. **Design the contract first (API-First)**
   - Define URL (lowercase, no HTTP verbs in name)
   - Define request body (camelCase params)
   - Define response structure (camelCase fields)
   - Confirm response codes: 200, 400, 401, 429
3. **Create/Update the Controller** — max 40 lines per method
4. **Create the Service layer** — business logic here
5. **Add parameterized SQL** — never concatenate user input
6. **Add pageIndex + pageSize** if returning a list
7. **Add error handling** — log to MongoDB, send email on error
8. **Write unit tests**
9. **Update README / API documentation**
```

---

## Slash Commands You Can Use

| Command | What It Does |
|---|---|
| `/goal` | Run a long task overnight without stopping |
| `/schedule` | Run a task on a timer or schedule |
| `/grill-me` | Interactive interview to plan a feature |
| `/learn` | Teach Antigravity something permanently |

### Example Usage:
- `/goal` → "Build the complete Transactions CRUD API with all endpoints, tests, and documentation"
- `/grill-me` → Antigravity will ask you questions to fully understand a feature before building it

---

## What Antigravity Will ALWAYS Do In This Project

Because of your `AGENTS.md`, Antigravity will **always**:

| Rule | Effect |
|---|---|
| No hardcoded values | Uses `appsettings.json` / env vars |
| Functions ≤ 40 lines | Splits large functions automatically |
| `using` statements | Wraps all `SqlConnection`, `SqlCommand` |
| Error logging | Adds MongoDB logging + email alerts |
| Parameterized queries | Never concatenates SQL strings |
| `pageIndex`/`pageSize` | Added to every list GET API |
| camelCase responses | All JSON responses use camelCase |
| No HTTP verbs in URL | `/transactions` not `/getTransactions` |

---

## Recommended Next Steps

1. ✅ **AGENTS.md is created** — rules are active now
2. 🔲 **Create a skill** for your most repeated task (e.g., "new API endpoint")
3. 🔲 **Try a feature request** — just describe what you want in plain English
4. 🔲 **Use `/grill-me`** when starting a complex feature to align on the plan first

---

## Quick Reference: File Locations

| Purpose | File |
|---|---|
| Project rules for Antigravity | `.agents/AGENTS.md` |
| Project skills for Antigravity | `.agents/skills/<name>/SKILL.md` |
| Global rules (all projects) | `C:\Users\admin\.gemini\config\AGENTS.md` |
| GitHub Copilot instructions | `.github/copilot-instructions.md` |
| API coding standards | `.github/CODING_GUIDELINES_API.md` |
| React coding standards | `.github/CODING_GUIDELINES.md` |
