# 🏰 AGE OF LANES — Unity 2D Game

Un joc 2D de tip **lane battler**, inspirat din *Age of War 2*: două baze (stânga/dreapta), trimiți minioni care se luptă automat, câștigi aur și XP, faci upgrade la unități și bază.  
Scopul: distruge baza inamică înainte să cadă a ta.

---

## 🎮 Pitch rapid
- **Gen:** Send-minions / Lane battler  
- **Engine:** Unity **2022.3 LTS**, Template **2D (URP optional)**  
- **Platformă:** PC (Windows, 16:9, 1080p)  
- **Echipă:** 3 developeri, obiectiv MVP în ~40h

---

## 🧠 Game Loop
1. Primești **aur/sec** și **bounty** la kill.  
2. Cheltui aur pentru a **spawna unități** (1/2/3).  
3. Unitățile se mișcă automat și atacă inamicii.  
4. Primești **XP** și faci **upgrade-uri** la unități și bază.  
5. Distrugi baza inamică pentru a câștiga meciul.

---

## ⚔️ Reguli principale
- Două baze cu **HP** (ex: 2000).  
- Fiecare bază are un **turret** auto-fire.  
- **O singură bandă** (MVP).  
- **AI adversar** care scalează în dificultate.  
- Unitățile se opresc la contact (melee lock).  
- Targetare: cel mai apropiat inamic din față.

---

## 💰 Economie & Progresie
- **Aur (AUR):** spawn unități, income pasiv + bounty.  
- **XP:** pentru Tech Tier și Upgrade-uri.  
- **Formule recomandate:**
	- `AUR/sec = 3 + 0.25 * (TechTier - 1)`
	- **Bounty:** 3–8 AUR / kill  
	- **XP/kill:** 1–3 XP  
	- **TierUp:** 50 / 125 / 250 XP

---

## 🧱 Unități MVP

| Tip      | Cost | HP  | DMG | Rate  | Viteză | Range | Rol               |
|----------|------|-----|-----|-------|--------|-------|-------------------|
| Soldat   | 25   | 90  | 10  | 1.0s  | 60     | 40    | ieftin, frontline |
| Tanc     | 60   | 240 | 16  | 1.2s  | 45     | 40    | tank, rezistent   |
| Arcaș    | 45   | 70  | 8   | 0.8s  | 60     | 200   | dps la distanță   |

**Scaling pe Tech Tier:** +10% HP & +10% DMG per tier.

---

## ⬆️ Upgrade-uri
- **Tech Tier I → III**: scaling global.  
- **Bază:**
	- HP +20% / tier
	- Turret DMG +10% / AtkRate -10%
- **Globale:**
	- *Infantry Training* – +10% HP unități  
	- *Sharp Blades* – +10% DMG unități  
	- *Logistics* – +10% AUR/sec  

---

## 🔥 Abilități
- **Q – Rally:** +20% Attack Rate 6s (CD 30s)  
- *(Stretch)* **W – Firestorm:** AoE 100 DMG (CD 40s)

---

## 🕹️ Controale
- **1/2/3:** Spawnează unități  
- **Q:** Abilitate Rally  
- **U:** Deschide Upgrade Panel  
- **ESC:** Pauză / Settings

---

## 🗂️ Structura proiectului (Unity)

```
Assets/
	Art/
	Audio/
	Prefabs/
		Units/
		Base/
		Projectiles/
	Scenes/
		Main.unity
		Game.unity
	Scripts/
		Core/
		Combat/
		Units/
		AI/
		Economy/
		UI/
		Abilities/
		Data/
	units.json
	upgrades.json
	tiers.json
```

---

## 🤖 AI – Curbă dificultate
- Buget inițial: 150 AUR / 30s  
- Crește cu +15% pe minut  
- Compoziție: 70% Soldați, 20% Arcași, 10% Tanc → gradual 40/30/30  
- CD global spawn: 0.6s

---

## 👥 Împărțirea taskurilor

### 👤 Dev A — Gameplay & Combat
- Mișcare unități + coliziune melee  
- Health/Damage/Death system  
- Turret auto-fire  
- Implementare unități din JSON  
- Abilitate Rally  

---

### 👤 Dev B — Economie, AI & Progresie
- Aur/sec + XP + Upgrade Manager  
- Tech Tiers & multiplicatori  
- AI Spawner & curba de dificultate  
- Win/Lose conditions + scor final
- Feedback & evenimente OnKill

---

### 👤 Dev C — UI/UX, Audio & Build
- HUD complet (aur, xp, tier, hp baze)  
- Panou upgrade-uri (U)  
- Animații + SFX spawn/hit/death  
- Pauză / Settings / Victory-Defeat  
- Build pipeline & polish final

---

## 🗓️ Plan 40h (2 sprinturi)

### Sprint 1 – MVP jucabil (~20h)
- A: Combat + 3 unități
- B: Aur/sec + AI v1 + Win/Lose
- C: HUD minimal + spawn + pauză  

✅ Poți câștiga/pierde un meci complet (~5 min)

### Sprint 2 – Polish & Upgrade-uri (~20h)
- A: Turret + Rally
- B: Upgrade panel + Tech tiers
- C: SFX + Victory/Defeat + build final  

---

## 🧪 Balancing
- TTK Soldat vs Soldat ≈ 9s  
- Time-to-Win: 4 min (Normal) / 7–8 min (Hard)  
- AUR/sec minim: 3  
- CD spawn: 0.8s / 1.2s / 1.5s

# ProiectCTIJ