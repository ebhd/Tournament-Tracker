# Seeded team data

Sources checked on 3 October 2026. These are fixed demonstration data, not a live feed. Coaches reflect the federation announcements below. Squad values are Transfermarkt estimates from the indexed 2026 squad snapshots available at lookup; they are not guaranteed current valuations or historical values for the seeded tournament editions. Values are stored in **euros**, not millions.

| Team | Coach | Squad value (EUR) | Coach source |
| --- | --- | ---: | --- |
| Belgium | Mark van Bommel | 547,500,000 | [Royal Belgian FA](https://royal-belgian-football-association.prezly.com/en) |
| France | Zinedine Zidane | 1,520,000,000 | [FFF](https://mediterranee.fff.fr/simple/place-a-la-gen-z/) |
| Spain | Luis de la Fuente | 1,220,000,000 | [RFEF](https://rfef.es/es/selecciones/tecnicos/luis-de-la-fuente) |
| Germany | Jürgen Klopp | 947,000,000 | [DFB](https://www.dfb.de/news/juergen-klopp-als-bundestrainer-vorgestellt) |
| Italy | Roberto Mancini | 833,500,000 | [FIGC](https://www.figc.it/en/national-teams/news/roberto-mancini-appointed-italy-head-coach-claudio-ranieri-named-figc-technical-director-and-club-italia-president-hq5v6w6o) |
| Portugal | Jorge Jesus | 1,010,000,000 | [FIFA](https://www.fifa.com/es/tournaments/mens/worldcup/articles/jorge-jesus-nuevo-seleccionador-de-portugal) |

## Valuation sources

- [Belgium, France, Spain and Portugal: Transfermarkt qualified nations overview](https://www.transfermarkt.co.uk/brazil-south-africa-japan-amp-co-the-28-nations-already-qualified-for-the-2026-world-cup/view/news/460240)
- [Germany: Transfermarkt](https://www.transfermarkt.co.uk/15-debutants-in-13-months-nagelsmanns-extensive-rebuild-of-the-germany-national-team/view/news/445023) (team summary snapshot)
- [Italy: Transfermarkt market value analysis](https://www.transfermarkt.com/italien/marktwertanalyse/verein/3376/sort/marktwert.desc)

## Logos

The images use Transfermarkt's public crest CDN: `https://tmssl.akamaized.net/images/wappen/big/{teamId}.png`. IDs: Belgium 3382, France 3377, Spain 3375, Germany 3262, Italy 3376, Portugal 3300. All six URLs returned HTTP 200 with PNG content at verification. Logos remain the property of their respective owners. If an image fails, the site displays country initials instead.

Both the EF seeder and in-memory repository use these same values. The existing development startup recreates and seeds the database; restarting the app applies the seed updates and also resets existing data, as before.
