# 🎮 GHID COMPLET - Cum să Faci UI cu Personaje și Prețuri

Asta-i un ghid pas-cu-pas pentru ÎNCEPĂTORI! Voi explica fiecare lucru pe rând.

---

## 📋 Ce Am Creat Pentru Tine

Trei fișiere JavaScript (C#) care fac tot automat:

1. **UnitSpawnerUI.cs** - Controleaza tot ce se vede pe ecran
2. **UnitSpawnButton.cs** - Controleaza fiecare buton individual (cap + preț)

**De ce două fișiere?** Și pentru cât de ușor e de modificat mai târziu!

---

## 🎯 PASUL 1: Creeaza un Canvas

Un Canvas e ca un tablă pe care pui butoanele.

**Cum:**
1. Click dreapta în **Hierarchy** (fereastra din stânga cu GameObject-urile)
2. Selectează **UI → Panel – TextMeshPro** (asta creează Canvas automat)
3. Se va crea automat:
   - Canvas (tablă principală)
   - Panel (dreptunghiul gol)

**Ce e Canvas?** E ca o foaie de hârtie pe care desenezi butoanele și textele pe ecran.

---

## 🎨 PASUL 2: Fă Paneul Pentru Butoanele de Personaje

Trebuie să-l redimensionezi și să-i dai o aranjare frumoasă.

**Cum:**
1. Selectează **Panel** din Hierarchy
2. În Inspector (dreapta), modify:
   - **Width**: 1200 (lătimea)
   - **Height**: 150 (înălțimea)
   - **Anchor**: Bottom Center (jos în mijloc)
   - **Offset Y**: 50 (distanța de jos)

3. Click dreapta pe Panel → Selectează **Layout → Horizontal Layout Group**
   - Asta e un component magic care aranjează butoanele în linie!

**De ce Horizontal Layout Group?** Pune automat butoanele în ordine, nu trebuie să le aliniezi manual.

---

## 🔘 PASUL 3: Creeaza Trei Butoane (Soldat, Tanc, Arcaș)

O să crezezi că-i prea mult, dar-i ușor!

**Pentru FIECARE buton:**

### 3.1 - Creeaza un Button
1. Click dreapta pe **Panel**
2. **UI → Button – TextMeshPro**
3. Se va crea **Button** cu **Text** sub el

### 3.2 - Redimensionează Butonul
1. Selectează **Button**
2. În Inspector:
   - **Width**: 200
   - **Height**: 140

### 3.3 - Adaugă Imagini și Text

Sub Button, vei vedea **Text (TMP)**. Trebuie mai multă structură.

**Asta e ce trebuie sub fiecare Button:**
```
Button
├── Image (Background - fundalul gri)
├── Image (Head - capul personajului) ← IMPORTANT!
├── Text - Nume (Soldat/Tanc/Arcaș)
└── Text - Preț ($25/$60/$45)
```

**Cum o faci:**

1. Selectează **Button**
2. **Click dreapta pe Button → UI → Image**
   - Se creează Image sub Button (asta e Background)
   - Dai-i un fundal (pe Inspector, alege o culoare sau sprite)

3. **Click dreapta pe Button → UI → Image** (din nou)
   - Se creează al doilea Image (asta e pentru Cap)
   - Numește-l **HeadImage**
   - Poziționează-l sus (Anchor: Top Center)
   - Redimensionează-l (100x100 pixeli, de obicei)

4. **Sterge textul vechi:** Selectează **Text (TMP)** și șterge-l

5. **Click dreapta pe Button → UI → Text – TextMeshPro** (pentru nume)
   - Numește-l **NameText**
   - Scrie "Soldat" în **Text** (Inspector)
   - Poziționează-l jos-stânga

6. **Click dreapta pe Button → UI → Text – TextMeshPro** (pentru preț)
   - Numește-l **PriceText**
   - Scrie "$25" în **Text**
   - Poziționează-l jos-dreapta

**Rezultat final:**
```
┌──────────────┐
│  [CAP IMG]   │  ← Imaginea personajului
├──────────────┤
│ Soldat  $25  │  ← Nume și preț
└──────────────┘
```

---

## ⚙️ PASUL 4: Adaugă Componenta UnitSpawnButton

Asta-i pasul magic! Componenta asta face butonul să funcționeze.

**Cum:**
1. Selectează **Button** (cel pentru Soldat)
2. În Inspector, click pe **Add Component**
3. Cauta și selectează **UnitSpawnButton**
4. Verzi că apar noi câmpuri:
   - Head Image
   - Price Text
   - Name Text
   - Spawn Button

5. **Trage și plasează** referințele:
   - **Head Image**: Trage imaginea capului în acest câmp
   - **Price Text**: Trage textul prețului
   - **Name Text**: Trage textul numelui
   - **Spawn Button**: Trage butonul însuși

**Ce face componenta?** Cand dai click pe buton, spawneaza un personaj. Și verifica dacă ai destul aur!

---

## 🎭 PASUL 5: Repeta Pentru Celelalte Două Butoane

Du-te înapoi la PASUL 3 și fă același lucru pentru:
- **Tank** (cost $60)
- **Archer** (cost $45)

**Sfat rapid:** După ce ai terminat cu Soldat, poti **copier și lipi** Buttonul (Ctrl+D în Hierarchy), apoi doar schimbi:
- Imaginea capului
- Textul numelui
- Textul prețului

---

## 🖼️ PASUL 6: Adaugă Sprite-urile (Capurile Personajelor)

Trebuie să ai imagini pentru cap.

**Unde să gasesti:**
- Folderul **UNIT HEADS** din Assets

**Cum:**
1. Selectează butonul Soldat
2. Selectează componenta **UnitSpawnButton**
3. Click pe câmpul **Head Image → Sprite**
4. Alege imaginea pentru Soldat din Assets

Repeta pentru Tank și Archer!

---

## 🎛️ PASUL 7: Configureaza UnitSpawnerUI

Asta e managerul general care controleaza TOT.

**Cum:**
1. Click dreapta în **Hierarchy → Create Empty**
2. Numește-l **UnitSpawnerUIManager**
3. Adaugă componenta **UnitSpawnerUI**
4. Verzi câmpuri cum ar fi:
   - unitButtons[0], unitButtons[1], unitButtons[2]
   - unitIcons[0], unitIcons[1], unitIcons[2]
   - unitCostTexts[0], unitCostTexts[1], unitCostTexts[2]
   - unitNameTexts[0], unitNameTexts[1], unitNameTexts[2]

5. **Trage butoanele** în aceste câmpuri (în ordine!)
6. **Trage imaginile capurilor** în unitIcons
7. **Trage textele preț** în unitCostTexts
8. **Trage textele nume** în unitNameTexts

---

## ✅ Gata! Ce Se Întâmplă Acum?

- ✅ Butoanele apar pe ecran cu capurile și prețurile
- ✅ Cand dai click, spawneaza un personaj
- ✅ Daca nu ai destul aur, butonul devine gri
- ✅ Prețurile se iau automat din **units.json**
- ✅ Cand duci ceva cu click, apare cooldown-ul

---

## 🐛 Daca Ceva Nu Merge

**Butonul nu reacționează:**
- Asigură-te că **Button** are componenta **Button** (nu doar Image!)
- Verifica ca OnClick lista are ceva (ar trebui sa aiba)

**Nu se vede imaginea capului:**
- Asigură-te că sprite-ul e selectat în **Head Image**
- Verifica că sprite-ul e în folder și nu-i șterse

**Pretul nu se schimba:**
- Asigură-te că **units.json** e în **Assets** folder

**Nu-mi dau seama unde e ceva:**
- Deschide Hierarchy și cauta cu Ctrl+Shift+F
- Sau pe email/Discord, postezi o poza din Hierarchy ta

---

## 📚 Vocabular Util

- **Canvas**: Tablă pe care sunt butoanele/textele
- **Button**: Buton (ala cu care dai click)
- **Image**: O imagine pe ecran
- **Text**: Text pe ecran
- **Component**: O mică programa atașata unui GameObject
- **Hierarchy**: Fereastra cu lista GameObject-urilor
- **Inspector**: Fereastra cu setările unui GameObject

---

## 🎓 Sfaturi Finale

1. **Salvează des!** (Ctrl+S)
2. **Testează după fiecare pas**
3. **Nu-ti fie frica sa experimentezi** - nu poți strica ceva permanent!
4. **Backup:** Inainte de marile schimbari, fă o copie a folderului ProiectCTIJ

---

Gata! Dacă nu-ți dau seama la vreun pas, întreabă-mă! 😊
