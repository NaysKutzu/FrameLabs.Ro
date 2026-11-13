# Cookie-uri și date personale

Verificări din 13 noiembrie 2026. Sunt nepotriviri față de ce cere, în principiu, legea 506/2004 și GDPR. ANSPDCP decide dacă merită o plângere. Probe: [headers-homepage.txt](../evidence/site/headers-homepage.txt), [privacy.txt](../evidence/site/privacy.txt), [privacy-shopify.txt](../evidence/site/privacy-shopify.txt).

## Ce se setează din prima

Am cerut pagina principală fără să dau click pe nimic. În răspuns apar, pe lângă cookie-uri de coș și limbă, `_shopify_analytics` și `_shopify_marketing`. Valorile nu le public aici, doar numele, în fișierul de headere.

În pagină e și scriptul Shopify `privacy-banner.js`. N-am stat într-un browser să văd dacă bannerul apare înainte ca aceste cookie-uri să fie puse. Ce e sigur: sunt deja în `Set-Cookie` la primul răspuns. Dacă analytics și marketing nu sunt strict necesare, ar trebui acord înainte.

## Politica scurtă

Pe https://framelabs.ro/pages/privacy (25 septembrie 2026), la cookie-uri, scrie doar că sunt unele necesare magazinului și „statistici de bază”. Nu e o listă, nu sunt durate, nu e temeiul, nu sunt terții (Shopify, Google Fonts, Cloudflare).

## Două texte, două povești

Aceeași politică scurtă zice că datele nu sunt folosite pentru marketing către terți. Politica lungă de la https://framelabs.ro/policies/privacy-policy, șablon Shopify, vorbește despre publicitate personalizată, parteneri de marketing și transferuri în afara Europei. Nu zic același lucru.

## Asistentul AI și HWID

Politica scurtă recunoaște că mesajele din asistentul din aplicație pleacă la „furnizorul serviciului AI”, fără nume, fără țară. În termeni, HWID e prezentat ca și cum n-ar fi dată personală. În politică e prelucrat pe interes legitim, ca să lege licența de un calculator. Împreună cu Discord și emailul de comandă, un identificator de hardware poate fi dată personală. Textele nu sunt aliniate.

Fonturile sunt trase de la `fonts.googleapis.com`. E un detaliu clasic de transfer, nu o sentință.
