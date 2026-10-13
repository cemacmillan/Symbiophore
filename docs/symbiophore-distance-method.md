# The Symbiophore distance method

**Status:** production in Symbiophore; copied (with different ring sizes) into Witness `SpatialScoreboard`, MindMatters `ProximityBandUtility` / discrete clock, Freymwerik band masks.  
**Audience:** anyone binning a relation where exact coordinates or exact sender identity do not change predicted output.

This note names the primitive we discovered while simplifying Symbiophore’s psychic field, then generalizes it. The living code is [`Source/Signals/`](../Source/Signals/). Witness already treats that stack as the spatial template ([`witness-direction.md`](../../Witness/docs/witness-direction.md)).

---

## 1. What we learned

RimWorld (and Unity) will happily convert, lerp, and round at every step. None of the floats we collected were useful. Player-legible outcomes sat in a **very small catalog** of bins (here: not more than ~18 mood magnitudes). Mixing sick and healthy emitters with continuous proxies produced **oscillation artifacts**, not information.

If:

- coordinates do not matter except as a **relative shell** between A and B,
- the **sender of record** does not matter except as a **configuration** (what A is emitting),
- the receiver only has a **finite set of states** that can display or act on the result,

then computing exact distance, unique thoughts per emitter, and continuous falloff **does not affect predicted results**. That work can be deleted.

The same pattern appears if you replace *distance* with *time* (phase / beat / cycle), *social closeness*, *attention*, or any other ordered relation that only has a handful of distinguishable shells.

---

## 2. The primitive (as implemented here)

Four shells, sampled rarely (~250 ticks), receiver-centric:

| Band | Cells | Relation meaning |
|------|-------|------------------|
| **Immediate** | 0–7 | Touch / interaction. Distance ignored. Effect 1:1 with A’s envelope. Any A in this ring is equivalent to any other. |
| **Near** | 8–29 | Variable. **Global max** among A’s in the ring; a few discrete steps of attenuation. |
| **Far** | 30–60 | Bare awareness. Collapses to 3 / 2 / 1 from **motion sign**, not fine distance. |
| **Beyond** | >60 | Occluded / out of range. Stop sustaining. Let the receiver’s existing state **decay** (vanilla thought duration). |

**Motion** is three classes in map space + time (no wormholes): approaching / steady / receding → −1 / 0 / 1. Sample last distance vs current; do not recompute a field every tick.

**A’s configuration** is an operational envelope (machine mode), not a continuous field:

- Healthy + temperature → discrete 1 / 3 / 7 / 12  
- Rain → +30  
- Injured (including sickness) → hard −3  
- Catastrophic (fire, critical health, …) → −80  

**Aggregation:** the receiver discovers the **strongest** signal in the innermost occupied ring (catastrophe wins). No stacking. Multiple A’s in Immediate are one relation.

**Spring:** guess the next displayed integer, then relax one step toward the target. Receding blocks ascent (time dominates decay). Mode jumps (rain / injured / catastrophe) snap.

**Projection:** displayed integer → one **shared Thought stage**. Refresh age if the stage is unchanged (medic walk should not destroy/recreate). PsychicSensitivity is **B’s capacity**, applied **once** by the game on that stage — never baked into A.

```text
A.envelope  ×  band(A,B)  ×  motion(A,B)
        →  target bin
        →  spring on B
        →  B.stage  ×  B.PsychicSensitivity
```

Identity of A is optional (bonding / named labels later). Mood math does not need it.

---

## 3. Generalized mechanism

Write the composition as:

```text
rel(A,B)  ⊗  config(A)  ⊕  capacity(B)  →  Π_B( lhs )
```

| Term | Meaning | Symbiophore instance |
|------|---------|----------------------|
| **rel(A,B)** | Ordered **shell** of a relation. Not coordinates. Not “who.” Distance, time-phase, social class, attention, duty-cycle cell — whichever axis the player (or the AI) can actually distinguish. | `DistanceBand` + `MotionClass` |
| **config(A)** | A’s **operational envelope**: the discrete things A can currently *be* that change the signal. | Health / rain / temp bins → BaseMood |
| **capacity(B)** | B’s **ability to receive** — gates or scales, never computed on A. | PsychicSensitivity; also “has mood,” humanlike, spawned |
| **⊗** | Band **selects a rule**, not a float multiply. Immediate = identity; Near = stepped map; Far = awareness counter; Beyond = zero / stop. Same for time: “this phase” vs “this beat” vs “this cycle.” | `TargetFor` |
| **⊕** | Capacity applied **on B, once**, after the discrete lhs exists. | XML `effectMultiplyingStat` only |
| **Π_B** | **Projection** into B’s **already-possible states**. The lhs is not a new continuous value; it indexes a catalog B already has (thought stages, FactAnswer, priming band, job eligibility). | `StageIndexForMood` → one memory |

If Π_B has N states, the lhs only needs enough bins to hit those states. Extra precision is wasted.

### 3.1 Relation is a ring, not a vector field

From B’s viewpoint, all A in the same shell are interchangeable for this predicate. That is why Immediate is a **ring of equivalent emitters** and Near uses **global max**. Exact sender is a *later* overlay (bonded psycaster preference), not the core relation.

Ego-centric rings (Witness) are the same idea with a smaller interaction shell (Touch / Presence / Witness / Beyond, Chebyshev ≤ 7). Psychic Far(60) is a **local scale choice**, not a different primitive.

### 3.2 Time is the same primitive

Replace cells with ticks:

| Spatial | Temporal (MindMatters clock) |
|---------|------------------------------|
| Immediate | This beat (15 ticks) — identity, no fade |
| Near | This phase (30) — still salient, stepped |
| Far | This cycle (120) — awareness / residual |
| Beyond | Older than the catalog — stop refresh, decay |
| Motion −1/0/1 | Phase advancing / holding / slipping |

`MmDiscreteClock` bins *when*; Symbiophore bands bin *where*. Both exist because the game’s delays mean most simulated detail never reaches an output state.

### 3.3 Capacity stays on the receiver

A does not know B’s PsychicSensitivity, Interactable bit, or priming window. Baking those into config(A) double-counts (we did this by accident: XML × C#) and makes A’s envelope unshareable across B’s.

Witness `ScoreCapacity` (Spawned, Awake, Interactable, Observable) is the same slot in the formula for *opportunity* rather than *mood*.

### 3.4 Projection is the point

The lhs is only interesting as an **index into B**.

| Domain | Π_B catalog |
|--------|-------------|
| Symbiophore | Thought stages (−80, −3, 1…12, 30) |
| Witness | `FactAnswer` Yes / No / Maybe; `CanInteract`; `IsClosingSocially` |
| MindMatters ZOI | Opinion class Dead / Cold / Live / Bonded; peacock ring; priming |
| Freymwerik | `BandMask` affordances |
| Jobs | Eligible / not this beat / cooldown |

If two lhs values project to the same B-state, they are the same bin. Collapse them.

---

## 4. Sampling recipe (the part that actually copies)

This is what other mods mean by “Symbiophore-style”:

1. **Staggered sample** (hash interval ~250). Accept stale.  
2. **Walk pawn lists**, not CellRect × thingGrid.  
3. **Bin** distance (or Chebyshev) into a tiny enum.  
4. **Motion** from sign(Δdist) vs last sample.  
5. **Store the band**, not the float.  
6. **Do not save** the spring/ring if it can rebuild (Witness scoreboard; Symbiophore `LinkSpring`).  
7. **Stop writing** when Beyond / zero; let existing receiver state age out.  
8. **One output row per receiver predicate**, not per sender.

Ring **size** is per domain (psychic 60 vs interaction 7). The recipe is shared; the cutoffs are not.

---

## 5. Variants in this ecosystem

| Place | rel(A,B) | config(A) | capacity(B) | Π_B |
|-------|----------|-----------|-------------|-----|
| **Symbiophore** (this repo) | Immediate/Near/Far/Beyond + motion | Envelope ints | PsychicSensitivity | Thought stage |
| **Witness SpatialScoreboard** | Touch/Presence/Witness/Beyond, Chebyshev ≤7 | (world facts, not psyche) | ScoreCapacity | DyadSnapshot / FactAnswer |
| **MindMatters ProximityBandUtility** | Touch/Presence/Witness, scan ≤7 | (psyche owns meaning) | humanlike / spawned | Peacock / ZOI collect |
| **MmDiscreteClock** | beat/phase/cycle | — | — | aligned social durations |
| **Freymwerik FactLayer** | BandMask from Witness bands | — | AffordanceGates | affordance mask |

Do **not** paste psychic Far(60) into Witness. Do **not** paste interaction Touch(1) into Symbiophore Immediate(7) without noticing they answer different questions (touch-as-max-effect vs melee-scale).

---

## 6. When *not* to use it

- You need the **named** A (this specific bonded creature, this specific witness of a crime). Then identity is part of Π_B, not collapsed in the ring.  
- Continuous **physics** (projectiles, cover, exact LOS for gunfire). Bins are for *meaning* and *opportunity*, not ballistics.  
- The catalog of B-states is actually large and player-visible (e.g. a debug slider). Then you have not finished collapsing.

Bonding, named thoughts, and fine LOS are **overlays** after the relation is binned.

---

## 7. A compact checklist for a new consumer

1. List B’s possible output states for this predicate. That is N.  
2. List A’s configurations that can change which of those N fire.  
3. List relation shells that change the rule (usually 3–4, plus Beyond).  
4. Put capacity on B only.  
5. Sample rarely; keep last rel + motion.  
6. Project; refresh-or-decay; never unique-per-A unless identity is in the catalog.

If after that you still want a float, you probably do not need it.
