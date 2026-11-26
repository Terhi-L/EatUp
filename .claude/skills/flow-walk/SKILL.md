---
name: flow-walk
description: Walk EatUp's main user flow in a real browser at phone and desktop size, then look at every screenshot like a user. Use before calling a change to anything on screen done, or when asked to walk the flow or check it like a user.
---

# Flow walk

`npm run walk` plays a user through the main loop (see the kitchen → add milk → cook now → mark the eggs eaten → see the month on Profile) at phone and desktop size and saves a screenshot after every step. Passing steps prove the flow works; the screenshots are for what a test can't see. The walk runs in Playwright's own browser against a fresh database, so the user's browser and data stay untouched.

**Input:** a Backlog task ID (`/flow-walk TASK-12`), or nothing to walk the flow as it is.

## Steps

1. **Read the ticket.** `backlog task view <id> --plain`. For each acceptance criterion a user could see, write down the screen and the size (phone, desktop or both) where it shows. Done when every visible criterion has a place. Without a task, the place list is the walk's own steps.
2. **Extend the walk** in `web/e2e/flow-walk.walk.ts` for every place the walk doesn't reach yet: a `test.step` that does what the user does, `expect`s what they should see, then calls `screenshot(page, '<step>')`. Seed the data the screen needs through the `kitchen` list. Done when every place from step 1 ends in a screenshot.
3. **Run** `npm run walk`. Done when both projects (phone, desktop) pass. A failing step is a regression (fix the code, rerun) or a change to what the user does or sees (ask the user first, below).
4. **Look** at every screenshot in `web/flow-walk-results/phone/` and `web/flow-walk-results/desktop/` by opening it, checking each against the list below and the place list from step 1. Done when every screenshot has a verdict: OK, or the problem in one line.
5. **Report** a table: step × phone × desktop, each cell OK or the problem, plus which criterion each screenshot proves. Put it in the task's implementation notes and the PR description, and give the user the folder path so they can open the screenshots themselves. Screenshots stay local (the folder is git-ignored and emptied on every walk).

## What to look for

- **Cut off:** text clipped, overlapping, or running off the screen.
- **Hidden:** a button or link under the bottom bar or the floating Add food button.
- **Wrong words:** a raw key like `inventory.add`, or a date in the wrong format. The walk runs in English; when a step you added switches to Norwegian or Finnish, also English left behind and long Finnish words breaking the layout.
- **Wrong place:** an item in a column that doesn't match its days left; a count that doesn't match the list.
- **Empty:** a blank card, a missing icon, a zero where a "nothing yet" message belongs.
- **The promise:** what the ticket says the user will see is actually there, at the size it names.

## Stop and ask the user

- The API build fails on locked files: `npm run dev` is holding them. Ask the user to stop it, then rerun.
- A screenshot looks wrong while its step passes, and the ticket doesn't say whether that's intended. Name the screenshot and ask.
- A step fails for a reason outside the ticket's change, and a rerun fails the same way.
- A step no longer matches because the change alters what the user does or sees, even when the ticket asks for it. Show the old and the new step side by side and what it means for the user, and change the step only after they agree. The walk is the user's picture of their app, so it changes with their consent.
