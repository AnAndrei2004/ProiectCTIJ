# ProiectCTIJ — Unity 2D Lane Battler (MVP)

Proiect Unity 2D tip **lane battler** (o singură bandă). Jucătorul și AI-ul trimit unități care se luptă automat între două baze. Scopul este distrugerea bazei inamice.

---

## ✅ Ce este implementat în acest moment

### Gameplay de bază
- Spawn unități jucător și inamic pe o singură bandă.
- Unitățile se deplasează automat, detectează ținte și atacă (melee sau ranged).
- Țintire în față, distanță calculată corect (edge-to-edge), cu stop la contact.
- Ranged cu proiectile și protecție anti-tunneling.

### Economie și progresie
- Aur pasiv: **3 aur/sec** (fix).
- Gold pentru kill (în funcție de unitate).

### Unități (preseturi din prefab/name)
- **Heavy:** Soldier / Knight / Merchant (HP mare, damage mare).
- **Light:** Thief (rapid, damage mic).
- **Ranged:** Peasant (ranged dacă are proiectil setat) și Priest (ranged).

### Baze
- Baze cu **300 HP** fiecare.
- Efect de foc sub 50% HP și explozie la distrugere.
- Jocul se termină la distrugerea unei baze.

### UI & UX
- HUD cu **Gold**.
- Bare de HP pentru unități (World Space) + bare dedicate pentru baze.
- UI pentru spawn unități cu cooldown vizual.
- Pauză (ESC).

### AI
- Spawn periodic de inamici cu pattern simplu (Heavy → Ranged → Light → Light).
- Există și un script alternativ de AI cu buget, dacă e folosit în scenă.

### Cameră
- Scroll pe X când mouse-ul e aproape de margini.

---

## ✅ Taskuri implementate (15)
1. Inițializare proiect Unity 2D (URP) și structură de bază.
2. Stack la baze: unitățile nu se mai blochează între ele când lovesc o bază (maxim de damage).
3. Scenă de meniu cu buton Play (încărcare scenă joc).
4. Sistem de spawn unități pentru player și enemy.
5. UI pentru spawn unități (butoane + costuri/cooldown).
6. Mișcare + targetare + atac melee pentru unități.
7. Atac ranged cu proiectile (inclusiv anti-tunneling).
8. Fix proiectil pentru Priest.
9. Bare de HP pentru unități (world space).
10. Baze funcționale (BaseUnit) cu HP și end game.
11. Efecte vizuale de foc și explozie la bază.
12. HUD pentru gold.
13. AI simplu pentru spawn inamici (pattern).
14. Cameră cu scroll pe margini (follow cursor).
15. Fixuri diverse: animații, culori inamici, materiale/prefaburi.

---

## 🎮 Controale
- **1 / 2 / 3** — Spawn unități (player)
- **Q** — Rally (placeholder: log)
- **U** — Toggle upgrade panel (placeholder: log)
- **ESC** — Pauză

---

## 🗂️ Structură proiect (rezumat)
```
Assets/
  Scenes/
    Menu.unity
    SampleScene.unity
  Scripts/
    Core/ (GameManager, InputManager, UnitSpawner, CameraController)
    Units/ (Unit, BaseUnit, Projectile)
    UI/ (HUD, Health Bars, UnitSpawnerUI)
  units.json
  upgrades.json
```

---

## 📌 Notițe importante
- Fișierele JSON există în proiect, dar **nu sunt încă încărcate la runtime**.
- Abilitatea Rally și panoul de upgrade sunt doar **placeholder** în input.

---

## ▶️ Rulare
1. Deschide proiectul în **Unity 2022.3 LTS**.
2. Rulează scena **Menu** sau direct **SampleScene**.