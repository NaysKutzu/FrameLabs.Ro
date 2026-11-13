# FrameLabs (framelabs.ro)

Am analizat site-ul [framelabs.ro](https://framelabs.ro/) și ce am putut scoate din aplicația lor Windows. Verificările sunt din 13 noiembrie 2026.

Ce găsești aici e părerea mea, pe baza unor surse publice și a scripturilor extrase din executabil. Nu e sfat juridic. Nu acuz pe nimeni de infracțiune, fraudă sau malware. Dacă am greșit un fapt, deschide un issue și arată sursa.

## Ce vinde site-ul

Un magazin Shopify, în lei, cu o aplicație Windows care promite mai mult FPS și mai puțin lag. Pachete: FREE, Silver, Gold, Diamond, pe o lună, trei luni, un an sau licență permanentă.

Prețurile de pe site, în ziua verificării: Silver 50-350 lei, Gold 100-500 lei, Diamond 150-750 lei, după durată. Pe pagina principală, la planul anual, apar 21 / 33 / 46 lei (adică prețul anual împărțit la 12). Cel mai scump lucru de pe listă era 750 lei (Diamond permanent). Livrarea e digitală: acces pe Discord și un `.exe` publicat pe GitHub.

## Cine e în spate

Date publicate chiar de ei pe site (termeni, confidențialitate, „Date de contact”) și confirmate la ANAF pe 13.11.2026:

- BTE RIDES S.R.L.
- CUI 50706577
- Registrul Comerțului J2024031539003
- Sediu: Str. Negoiu nr. 44, Municipiul Craiova, județul Dolj, 200664
- Înmatriculată pe 15.10.2024
- Email pe site: contact@framelabs.ro

Mai multe despre firmă și CAEN: [docs/firma-si-caen.md](docs/firma-si-caen.md).

## Ce am găsit

CAEN. La ANAF, pe 13.11.2026, activitatea principală e 5829 (editare alte produse software). Pe identitate.ro, înainte de 07.11.2026 era 4933 (transport de pasageri la comandă). Pe o listă FirmeDEX, dată ca venind din Registrul Comerțului (26.07.2026), apar deja 6201, 6202, 6312 și 5829. Deci nu pot spune că fac software fără CAEN. Cel mult, principalul a fost ținut pe transport până foarte târziu.

Inactivitate fiscală. Tot la ANAF, firma figurează inactivă între 13.10.2025 și 11.09.2026. Site-ul pare să fi existat în intervalul ăsta: termenii sunt datați 26.05.2026, sitemap-ul are pagini din aprilie-august 2026, iar pe homepage sunt recenzii din martie-mai 2026. N-am văzut o factură din perioada aia, deci e un indiciu, nu o dovadă de vânzare. [docs/inactivitate-anaf-efactura.md](docs/inactivitate-anaf-efactura.md)

e-Factura. În politica de confidențialitate scrie că facturile pleacă la ANAF prin e-Factura. API-ul ANAF zice că firma nu e înscrisă în sistem. Cele două se bat cap în cap.

Retragere și rambursare. Termenii zic că, odată activat accesul, nu mai primești banii înapoi. Politica de rambursare zice că ai 14 zile, dacă n-ai aplicat nicio optimizare. Același comerciant, două variante. Asta e treabă de ANPC. [docs/protectia-consumatorului.md](docs/protectia-consumatorului.md)

Alte lucruri de pe site: datele firmei există pe paginile legale, dar nu în footer-ul de pe homepage; „Contact” te duce pe Discord; pagina `/pages/contact` e aproape goală. Pe homepage scrie „Peste 5.000 de gameri” și „+5-25% FPS prin tweaks kernel”. N-am găsit dovezi independente pentru cifrele astea și n-am măsurat FPS.

Cookie-uri. La prima cerere HTTP, fără să apeși nimic, apar `_shopify_analytics` și `_shopify_marketing`. Au și două politici de confidențialitate care nu zic același lucru despre marketing. [docs/gdpr-cookies.md](docs/gdpr-cookies.md)

Aplicația. Scripturile din exe pot opri actualizările automate Windows, SmartScreen și notificările Defender (nu am găsit oprirea antivirusului în sine). Pot schimba DNS-ul pe toate plăcile de rețea. Din binar reiese că aplicația poate trage scripturi de pe `apis.framelabs.ro` și le poate rula ca administrator. În comentarii, cei care au scris revert-ul recunosc că, dacă nu țin pasul, butonul Revert „devine tot mai mult o minciună”, și că valorile implicite Windows sunt „varianta mai sigură pentru user”. O parte din fișierele extrase sunt tocmai jumătatea de anulare, nu „optimizarea” în sine. Detalii, script cu script: [docs/analiza-aplicatie.md](docs/analiza-aplicatie.md)

## De ce zic că nu merită banii

E părerea mea, pe ce se vede în cod și pe prețurile de pe site. Produsul arată a o înșiruire de tweak-uri vechi, înfășurate într-o interfață, cu comentarii și pete care miros a cod generat cu AI. N-am găsit o ștampilă de tip „made with Codex”, deci nu pot spune cu ce unealtă a fost scris. Pot spune că nu merită, după mine, nici 50 de lei, darămite 750. Argumentele sunt în [docs/de-ce-nu-merita.md](docs/de-ce-nu-merita.md).

## Ce n-am putut verifica

- certificatul constatator de la Registrul Comerțului
- lista completă de CAEN pe Termene sau RisCo (cer un cont) și pe listafirme.ro (403)
- WHOIS-ul domeniului și arhiva Wayback
- checkout-ul: n-am dat nicio comandă, deci nu am văzut prețul final nici vreun bifat pentru retragere
- o factură anume; orice sumă care nu e pe listă trebuie dovedită separat
- rularea sau semnătura `FrameLabs.exe`
- dacă recenziile și cei „5.000 de gameri” sunt adevărați
- ce adrese DNS pune scriptul în practică
- un test de FPS făcut de mine

## Restul dosarului

- [docs/firma-si-caen.md](docs/firma-si-caen.md): firma și CAEN
- [docs/inactivitate-anaf-efactura.md](docs/inactivitate-anaf-efactura.md): inactivitate fiscală și e-Factura
- [docs/protectia-consumatorului.md](docs/protectia-consumatorului.md): retragere, clauze, prețuri, afirmații de pe site
- [docs/gdpr-cookies.md](docs/gdpr-cookies.md): cookie-uri și politici
- [docs/analiza-aplicatie.md](docs/analiza-aplicatie.md): ce face fiecare script pe PC
- [docs/de-ce-nu-merita.md](docs/de-ce-nu-merita.md): de ce nu merită, după mine
- [docs/cum-reclami.md](docs/cum-reclami.md): unde poți sesiza (ANPC, ANSPDCP, ANAF)
- [docs/surse.md](docs/surse.md): URL-urile și data verificării
- `evidence/`: copii text/JSON din 13.11.2026
- `embedded_scripts/` și `src_decompiled/`: materialul scos din aplicație

Dacă vrei să te plângi la o autoritate, uită-te în [docs/cum-reclami.md](docs/cum-reclami.md). Nu am depus nicio plângere.
