---
id: TASK-22
title: Confirm eating, throwing out and deleting with a second tap
status: To Do
assignee: []
created_date: '2026-11-26T13:31:00Z'
labels:
  - frontend
  - design
milestone: m-1
dependencies:
  - TASK-21
priority: medium
type: enhancement
ordinal: 11700
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
One tap on an inventory row's eaten or bin button records the food at once, and Delete on the edit screen removes the item, so a slip records the wrong outcome and skews the waste statistics. The product owner wants an extra click that says what the action does before it happens (2026-11-26). A two-step button gives that without a dialog in the way; an undo bar was the alternative considered (one tap, then "Undo" for a few seconds), which would need undo support in the API.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 The first tap on a row's eaten or bin button turns it into a labelled confirm ("Eaten?" or "Throw out?") and records nothing; a second tap records it
- [ ] #2 An unconfirmed button goes back to normal after a few seconds or when the user taps elsewhere, without recording anything
- [ ] #3 Every button on the edit screen that records or deletes the item asks the same way before acting
- [ ] #4 It works by touch on phone and by mouse and keyboard on desktop, and the confirm text is in en, nb and fi
- [ ] #5 The flow walk shows the confirm step at phone and desktop size
<!-- AC:END -->
