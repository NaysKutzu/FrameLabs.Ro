# Inactivitate fiscală și e-Factura

Date din API-ul public ANAF, 13 noiembrie 2026. Fișierele sunt în [evidence/anaf/](../evidence/anaf/). Interpretarea o face ANAF, nu eu.

## Ce zice ANAF

Am întrebat serviciul PlatitorTvaRest v9 pentru CUI 50706577, pe mai multe zile:

- 01.10.2025: încă nu era inactivă
- 01.12.2025 și 01.09.2026: inactivă
- 13.11.2026: din nou activă

În înregistrare apar data de inactivare 13.10.2025 (publicată 15.10.2025) și data de reactivare 11.09.2026. Deci intervalul e 13.10.2025 - 11.09.2026. Pe 13.11.2026 firma e activă, nu e plătitoare de TVA și nu e înscrisă în RO e-Factura.

## De ce zic că site-ul exista în perioada aia

Nu am o factură din interval. Am doar urme pe site:

- termenii sunt datați 26 mai 2026 ([termeni.txt](../evidence/site/termeni.txt))
- în sitemap, `/pages/contact` e din 16.04.2026, recenziile din 03.05.2026, termenii din 26.05.2026, iar paginile de tip „optimizare-...” din 08.08.2026 ([sitemap_pages.xml](../evidence/site/sitemap_pages.xml))
- pe homepage sunt recenzii datate 21.03.2026, 28.04.2026, 10.05.2026, 19.05.2026. Dacă sunt autentice, e altă discuție; n-am verificat.

Dacă în perioada de inactivitate s-a vândut ceva, ANAF e locul unde se verifică.

## e-Factura

Politica de pe https://framelabs.ro/pages/privacy (25 septembrie 2026) zice că facturile merg, prin SmartBill, în sistemul e-Factura, „așa cum cere legea”. API-ul ANAF, în aceeași zi în care m-am uitat, zice că firma nu e înscrisă. Pentru facturile către persoane fizice, transmiterea prin RO e-Factura e, în principiu, obligatorie din 1 ianuarie 2025. Excepțiile și amenzile sunt treaba ANAF. Eu semnalez doar că textul de pe site și starea din ANAF nu se potrivesc.
