---
name: lunar-engine-refactor
description: Use ONLY when refactoring LunarEngine through its 12-phase architectural migration. Before each phase, audits ARCHITECTURE.md against current code, creates/updates task.md, and manages a phase checklist.
---

# LunarEngine Refactoring Skill

## When to use
Use this skill at the start of every phase of the LunarEngine architectural migration described in `LunarEngine/docs/ARCHITECTURE.md`. If you are about to modify, create, or delete LunarEngine source files as part of the 12-phase plan, follow this skill first.

## Pre-phase ritual (MANDATORY)
Before writing or editing any code for a phase, you MUST perform the following steps using the available tools:

1. **Read the architecture document**
   - Read `LunarEngine/docs/ARCHITECTURE.md`.
   - Note the target design for the current phase.

2. **Read the latest audit**
   - Read `LunarEngine/docs/ARCHITECTURE_AUDIT.md` (if it exists).
   - This captures the last known conflicts.

3. **Review current codebase**
   - Use `glob` and `grep` to find files relevant to the current phase.
   - Use `read` to inspect key files.
   - Identify any NEW conflicts or drift that appeared since the last audit.

4. **Identify conflicts**
   - Compare what you see in code against the phase's target state in ARCHITECTURE.md.
   - Summarize the conflicts in your reasoning, but do NOT modify code yet.

5. **Update the audit (if needed)**
   - If you discover significant new drift, append a dated section to `LunarEngine/docs/ARCHITECTURE_AUDIT.md` describing it.

## Task tracking
After the audit is complete, manage tasks before coding:

1. **Create or read `task.md`**
   - Look for `task.md` in the repo root (`D:\_Keep\Active Projects\Silk.NET-Game-Engine\task.md`).
   - If it does not exist, create it with a checklist of all 12 phases from ARCHITECTURE.md.
   - Format example:
     ```markdown
     # LunarEngine Refactor Task List

     ## Phase 1: Hygiene and bug fixes
     - [ ] Fix all 10 bugs listed in ARCHITECTURE.md
     - [ ] Remove all dead code files
     - [ ] Normalize using statements
     - [ ] Verify `dotnet build -c Debug` and `dotnet run -c Debug`

     ## Phase 2: Core module extraction
     - [ ] Create ServiceContainer and ServiceDescriptor in Core/
     - [ ] Replace Singleton<T> usage with service registration
     - [ ] Make EventBus<T> instance-based instead of static
     - [ ] Move TimeStep and Logger to Core/
     ```

2. **Mark current phase**
   - In `task.md`, mark the phase you are about to start as `in_progress`.
   - Mark any previous phases as `completed` if they are truly done (build + run verified).

3. **Update conversation todos**
   - Use the `todowrite` tool to create/update the current task list.
   - Mirror the `task.md` checklist in the `todowrite` call.
   - Set exactly one item to `in_progress`; set completed items to `completed`.

## Phase execution rules
Once the audit and task setup are done, proceed with the phase:

1. **Make minimal changes** to achieve the phase goal.
2. **Do NOT rename namespaces** unless the current phase explicitly includes namespace consolidation.
3. **Follow existing code style** in the project.
4. **Build and run after every significant change**:
   - `dotnet build -c Debug`
   - `dotnet run -c Debug`
   - If either fails, fix before proceeding.
5. **Update AGENTS.md** if you change build steps, directory structure, or conventions mentioned there.

## Post-phase verification
After completing a phase:

1. Run a final `dotnet build -c Debug` and `dotnet run -c Debug`.
2. Update `task.md`:
   - Mark the phase as `completed`.
   - Mark the next phase as `pending` (or `in_progress` if you are continuing immediately).
3. Use `todowrite` to mark the phase task as `completed` and set the next phase as `in_progress`.

## Important reminders
- Never skip the pre-phase audit, even if you think you already know the state.
- Never modify code during the audit step.
- Never leave the build broken at the end of a phase.
- If a phase is too large to complete in one session, stop after a sub-task, update `task.md` with a partial-progress note, and set the todo to `in_progress`.
