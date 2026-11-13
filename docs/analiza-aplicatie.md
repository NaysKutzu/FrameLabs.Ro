# Ce face aplicația pe calculator

Materialul din depozit nu e „sursa oficială a site-ului”. Sunt 19 scripturi PowerShell scoase din executabil (`embedded_scripts/`), decompilarea serviciului de fundal (`src_decompiled/FrameLabsISLC_Program.cs`) și un pseudocod Ghidra pentru bucățile care lansează PowerShell, vorbesc cu API-ul și se pun la pornirea Windows (`src_decompiled/framelabs_ghidra_pseudocode_suspicious_funcs.c.txt`).

N-am rulat exe-ul ca să documentez asta. Nu zic că e malware. Zic ce se vede în fișiere. Unele extracte sunt tăiate la cap sau la coadă; unde e cazul, o scriu.

Aplicația publicată de ei, pe 13.11.2026: https://github.com/bte-stuff/FrameLabs, release v3.3.2 din 9.11.2026, fișier `FrameLabs.exe`. Contul `bte-stuff` își pune ca blog https://framelabs.ro. API-ul de la https://apis.framelabs.ro/ a răspuns `{"status":"ok","service":"FrameLabs API"}`.

Un lucru important, de la început: o parte din scripturile de aici nu „optimizează”, ci dau înapoi. Perechile 14/15, 16/17, 18/19 sunt clar aplicare și anulare. Scripturile 08-13, pe ce conțin, arată tot a anulare (șterg valori, pun înapoi default-urile Windows). Optimizările în sine pot veni, măcar în parte, de pe server, de la `/api/optimization/script`. Asta reiese și din șirurile din binar.

## Scripturile, unul câte unul

### script_01 (`embedded_scripts/script_01_off18438d.ps1.txt`)

Creează un punct de restaurare Windows (`Checkpoint-Computer`). Ca să poată face unul imediat, pune temporar `SystemRestorePointCreationFrequency` pe 0 și, dacă trebuie, pornește System Restore și mărește spațiul de shadow copy cu `vssadmin`. Apoi pune înapoi setarea veche. La final verifică dacă punctul chiar s-a creat în ultimele două minute.

E treaba standard pe care ți-o zice orice ghid de tweak-uri: fă un punct de restaurare înainte. Windows știe să facă asta singur, din Setări sau cu `Checkpoint-Computer`. Extractul începe în mijlocul unei funcții, deci lipsește capul fișierului. Riscul e mic, dacă punctul chiar se creează; dacă eșuează și tu tot apeși pe optimizări, n-ai la ce te întoarce.

### script_02 (`embedded_scripts/script_02_off1850a3.ps1.txt`)

Primește o listă de nume (`%1`) și încearcă să șteargă acele aplicații Windows (Appx) pentru toți utilizatorii, plus pachetele „provisioned” (cele care revin la un user nou). Comentariile, destul de lungi, povestesc un bug de pe 24.09 și că „la vreo 9% din rulări” rămâneau aceleași 12 aplicații. Extractul e tăiat la coadă.

Asta e debloat clasic. Același tip de treabă o fac, gratuit, Chris Titus WinUtil, Sophia Script, Win11Debloat și o grămadă de scripturi de pe GitHub. Riscul: poți pierde aplicații de sistem pe care le mai voiai; pe unele build-uri, ștergerea Appx e capricioasă, exact cum zic și comentariile.

### script_03 (`embedded_scripts/script_03_off185b2f.ps1.txt`)

Pune o politică de firmă: `TurnOffWindowsCopilot` în `HKLM`. Un comentariu explică de ce nu atinge `HKCU`: scriptul rulează ca admin, deci `HKCU` ar fi al administratorului, nu al tău. Tot un tweak des întâlnit, inclus în ghidurile de „scoate Copilot”. Riscul e mic, în afară de faptul că nu mai ai Copilot.

### script_04 (`embedded_scripts/script_04_off185cd2.ps1.txt`)

Dacă există `winget`, dezinstalează în tăcere `Microsoft.Copilot` și pachetul din Store `9NHT9RB2F4HD`, cu timeout de 60 de secunde. Continuarea lui script_03, tot pe calea obișnuită. Riscul e ca `winget` să șteargă și altceva dacă identificatorul se schimbă, dar aici e țintit.

### script_05 (`embedded_scripts/script_05_off186099.ps1.txt`)

Ia o a doua listă (`%2`) și, pentru programele Win32 care se potrivesc, rulează `QuietUninstallString` din registru, cu timeout de 90 de secunde. La sfârșit scuipă un rând de tip `FLBLOAT|sters=...`, ca să-l poată citi aplicația. Comentariile zic explicit că rezumatul e „ca să-l poată citi clientul” și că pun și versiunea de Windows.

Dezinstalare silențioasă de programe, pe o listă venită din afara scriptului. Riscul depinde de listă: poți pierde un program pe care îl foloseai. Timeout-ul și `cmd.exe /c` pe un șir din registru sunt rețeta obișnuită, nu e ceva magic.

### script_06 (`embedded_scripts/script_06_off187950.ps1.txt`)

Șase rânduri. Pe toate adaptoarele de rețea care sunt „Up”, pune DNS-ul pe două adrese injectate la rulare (`%1`, `%2`). N-am prins ce adrese sunt în practică.

Poți face același lucru din Setări, sau cu un ghid „pune Cloudflare / Google DNS”. Pe site au o secțiune „Network Boost” care testează câteva DNS-uri. Riscul e mai serios decât la Copilot: tot traficul de rezolvare a numelor trece prin serverele alea, pe toate plăcile active, inclusiv VPN sau hotspot dacă sunt sus. Dacă adresele sunt cele publice, e o comoditate. Dacă nu, e un punct de control asupra a ce site-uri îți rezolvă PC-ul.

### script_07 (`embedded_scripts/script_07_off2bfd0aa.ps1.txt`)

Aici sunt doar două funcții, `Del-Reg` și `Set-Dword`, plus un comentariu gol. Extractul arată incomplet. Probabil e un preambul folosit de scripturile de după el. Singur, nu face nimic vizibil pe sistem.

### script_08 (`embedded_scripts/script_08_off2bfd297.ps1.txt`)

Șterge valorile `bcdedit` `useplatformtick` și `disabledynamictick`, și cheia `GlobalTimerResolutionRequests`. `bcdedit /deletevalue` înseamnă „scoate setarea”, deci arată a revert, nu a aplicare. Tweak-urile de timer sunt un subiect vechi în ghidurile de latență; unelte gratuite: TimerResolution, scripturi publice care folosesc bcdedit, și bcdedit însuși, din documentația Microsoft. Riscul, când le aplici (nu când le ștergi), e instabilitate pe unele sisteme.

### script_09 (`embedded_scripts/script_09_off2bfd44a.ps1.txt`)

Pune `NetworkThrottlingIndex` pe 10 și `SystemResponsiveness` pe 20, apoi șterge `TcpAckFrequency`, `TCPNoDelay` și `DefaultTTL`. 10 și 20 sunt, de fapt, valorile implicite Windows. Ghidurile de „gaming” pun de obicei throttling-ul pe `0xFFFFFFFF` și responsiveness pe 0. Deci și ăsta arată a script de pus înapoi default-urile, nu a „boost”.

### script_10 (`embedded_scripts/script_10_off2bfd64c.ps1.txt`)

Pornește hibernarea (`powercfg /h on`), șterge câteva chei de power throttling, pune USB selective suspend pe Enabled (1, adică default) și PCIe ASPM pe 1 la priză / 2 pe baterie. Un comentariu zice că până la versiunea 3.3 puneau 2 și la priză, „economie maximă pe magistrală”. Iarăși, arată a corecție / revert. `powercfg` e unealta Microsoft. Riscul, pe varianta de aplicare (nu pe asta), e că un laptop pe baterie se poate comporta ciudat dacă forțezi planul de energie.

### script_11 și script_12 (`embedded_scripts/script_11_off2bfdb93.ps1.txt`, `script_12_off2bfdecf.ps1.txt`)

Merg prin clasele de plăci NVIDIA/AMD și *șterg* valori de tip PowerMizer, ULPS, preemption, MSI Mode. Comentariul din script_12 zice clar că un 0 pus cu forța ar fi „altceva decât am găsit”, deci preferă să șteargă valoarea, ca să se întoarcă la ce era înainte. Tot revert. Tweak-urile GPU din registru circulă de ani de zile pe forumuri și în „NVIDIA inspector / registry tweak” guides. Riscul, când le aplici, e artifact, crash sau un driver care le rescrie la următorul update.

### script_13 (`embedded_scripts/script_13_off2bfe759.ps1.txt`)

E scriptul mare de revert. Extractul începe în mijloc. Anulează o listă lungă de politici: widgets, animații, CDP, spooler, power throttling, UDP offload, paging pe kernel, și, de la linia 46, securitatea:

```
# Pachetul le opreste ca sa nu apara nimic peste joc. La revert revin pe
# valorile Windows, care aici sunt si varianta mai sigura pentru user.
```

De acolo repornește SmartScreen, scoate blocarea notificărilor Defender și Firewall, șterge `NoAutoUpdate` (adică pune la loc actualizările automate), readuce notificările de update, pune înapoi SMB2 pe varianta Diamond. La liniile 10-12:

```
# Fiecare linie anuleaza exact ce pune pachetul. Fara asta butonul Revert
# devine tot mai mult o minciuna cu fiecare versiune.
```

Asta e recunoașterea lor, în comentariu, că revert-ul rămâne în urmă dacă nu e ținut la zi, și că Windows-ul „curat” e mai sigur decât ce pune pachetul. Pe site, Silver anunță deschis „Disable Updates”. Restul (SmartScreen, notificări) nu e la fel de vizibil în oferta de pe homepage. Riscul, pe varianta aplicată, nu pe revert: mai puține patch-uri, mai puține avertismente.

### script_14 și script_15 (`script_14_off196570.ps1.txt`, `script_15_off196da0.ps1.txt`)

14 pune `AllowTelemetry` pe 0, oprește serviciile DiagTrack și dmwappushservice, dezactivează task-urile de Application Experience și CEIP. 15 le aduce înapoi (`AllowTelemetry` 3, servicii pe Automatic). Debloat / confidențialitate de manual. Același meniu e în O&O ShutUp10, Sophia Script, WinUtil, „privacy tweaks” de pe GitHub, tot gratuit. Riscul: unele diagnostice Windows nu mai merg; pe 15, le pornești la loc.

### script_16 și script_17 (`script_16_off1974a0.ps1.txt`, `script_17_off197910.ps1.txt`)

16 oprește SysMain (Superfetch), prefetch-ul și Superfetch-ul din registru. 17 le pornește la loc. Tweak vechi, tot pe lista „gaming”. Pe SSD efectul e discutabil; pe HDD, oprirea SysMain poate încetini deschiderea programelor. Se face din `services.msc` în două minute, fără să plătești.

### script_18 și script_19 (`script_18_off197d10.ps1.txt`, `script_19_off1986d0.ps1.txt`)

18 oprește Cortana, căutarea web, Bing în Start, istoricul de căutare. 19 șterge aceste politici, deci le dă înapoi. Iarăși, un clasic din ghidurile de debloat. Riscul e că Start-ul nu mai caută pe web, ceea ce unii vor, alții nu.

## Serviciul de fundal (FrameLabsISLC)

`src_decompiled/FrameLabsISLC_Program.cs`, cam 610 linii. În metadate e descris ca serviciu de fundal pentru ISLC, timer resolution, detectare de jocuri și priorități de proces. Cam la fiecare 3 secunde:

- se uită la procesele deschise, după o listă de nume (FiveM, GTA, CS2, Valorant, Minecraft etc.)
- dacă găsește un joc, îi urcă prioritatea la High și încearcă să-l scoată de pe CPU 0
- coboară prioritatea la chestii de fundal (browser, OneDrive, Teams etc., pe o listă)
- apelează `NtSetTimerResolution`
- dacă RAM-ul liber scade sub un prag (1 GB, în logica de acolo), golește standby list

Asta e, aproape pe față, ce face Intelligent Standby List Cleaner (ISLC) al lui Wagnard, plus unelte ca TimerResolution și Process Lasso, toate disponibile gratuit sau cu un trial. Pe site, pachetul Gold e vândut cu „FrameLabs Power Plan + ISLC”. Dacă e sau nu o problemă de marcă, nu am verificat la OSIM și nu compar un cod cu altul rând cu rând.

În `DefaultGamePatterns` (liniile 67-71), ultima regulă, pe linia 70, e `"*.exe"`. Funcția `MatchesPattern` (liniile 238-245) tratează `*` doar dacă e la sfârșit. `"*.exe"` nu se potrivește cu nimic serios. E un rest mort, nu un „ia toate procesele”.

## Ce face binarul cu serverul și cu pornirea Windows

În pseudocodul Ghidra apar, printre altele (vezi și [evidence/site/binar-stringuri-api.txt](../evidence/site/binar-stringuri-api.txt)):

- `https://apis.framelabs.ro`
- `/api/tweaks/definitions`
- `/api/optimization/script`
- `/api/update/manifest`
- `/api/integrity/check`
- `powershell.exe`, `schtasks.exe`
- cheile `...\CurrentVersion\Run` din HKCU și HKLM

Pe românește: aplicația poate lua de la ei lista de tweak-uri și scriptul de aplicat, îl poate rula cu PowerShell ca administrator, se poate pune la startup și poate folosi Task Scheduler. Dacă mâine schimbă scriptul de pe server, tu rulezi varianta nouă fără să vezi sursa înainte. Asta cere încredere în operator. Nu e o dovadă că au livrat ceva rău intenționat.

N-am găsit în materialul ăsta chei API sau parole. „token” și „hwid_sha256” apar ca nume de câmpuri, la rulare. Nu e un site scris de ei cu SQL la vedere (magazinul e Shopify), deci n-am căutat SQLi aici.

Pentru cine cumpără, întrebarea e alta: ți se spune destul de clar, înainte de plată, că ți se pot opri update-urile, că ți se poate schimba DNS-ul și că o parte din ce rulează ca admin poate veni de pe serverul lor?
