# Kotlin Multiplatform implementációs konvenciók

## Hatókör és módosítás előtti ellenőrzés

Ez a fájl a teljes `MobileApp` könyvtárra és annak almappáira vonatkozik. Az AI agent minden módosítás előtt köteles figyelembe venni.

- Olvasd el ezt a fájlt és az érintett könyvtárak további `AGENTS.md` fájljait. A szűkebb hatókörű szabály az adott könyvtárban elsőbbséget élvez.
- A szülőkönyvtár C#-specifikus előírásai helyett Kotlinban az itt leírt konvenciókat alkalmazd. Az általános munkavégzési és dokumentációs szabályok továbbra is érvényesek, az itt megadott pontosításokkal.
- Módosítás előtt vizsgáld meg az érintett forrásokat, hívási helyeket, modulfüggőségeket és a meglévő változtatásokat. Mások munkáját őrizd meg.
- Az új és módosított kódrészletek kövessék ezeket a szabályokat. A meglévő eltérő formázás nem követendő minta, de teljes fájlok vagy a projekt tömeges átformázását csak külön kérésre végezd el.
- Külső API, override vagy generált kód által meghatározott szerződést tarts meg. Generált fájl helyett annak forrását vagy generátorát módosítsd.

## Kotlin és Gradle Kotlin DSL formázás

### Paraméterek és argumentumok

- Paraméter- és argumentumlisták utolsó eleme után soha ne legyen vessző (`trailing comma`), több soros listánál sem.
- Ez vonatkozik a függvényekre, konstruktorokra, annotációkra, generikus paraméterlistákra és lambda-paraméterlistákra is.
- Függvény- és konstruktorhívásnál minden argumentumot név szerint adj át, amikor ezt a Kotlin engedi, egyetlen argumentum esetén is.
- A paraméter nevét a tényleges deklarációból ellenőrizd; ne találj ki paraméterneveket.
- Pozicionális argumentum csak akkor maradjon, ha a név szerinti átadást a nyelv vagy az API nem támogatja. Ilyen például a Java-metódus, a függvénytípusú érték hívása és a vararg elemek egyenkénti átadása. Ha egy vararg paramétert név szerint adsz át, a megfelelő tömböt használd.
- A zárójel után írt trailing lambda, az infix hívás és az operátorszintaxis megtartható. A zárójelen belüli többi argumentumot ilyenkor is név szerint add át, ahol lehetséges.
- A teljes MobileApp projekt minden saját függvénytípus-deklarációjában minden bemeneti paraméter kapjon beszédes nevet. Ez minden modulra, source setre és láthatóságra érvényes: callbackekre, konstruktor- és függvényparaméterekre, property-kre, lokális változók explicit függvénytípusaira, typealiasokra és visszatérési típusként megadott függvénytípusokra is. A szabály az összes meglévő és új deklarációra vonatkozik; a meglévő névtelen paramétereket is el kell nevezni az egész projektben. A neveket a tényleges szerep alapján válaszd meg; a paramétertípusokat, sorrendet és visszatérési típust ennek során őrizd meg.
- Függvénydeklaráció, elsődleges konstruktor és hívás behúzás nélkül legfeljebb 130 karakterig maradjon egy sorban. Hosszabb esetben minden paraméter vagy argumentum külön sorba kerüljön, a záró zárójel pedig a deklaráció vagy hívás behúzásának szintjére.

Példa a `TrackQueueCommandHandler` saját metódusának hívására:

```kotlin
addLoading(
    value = value,
    loading = false
)
```

Példa az egész projektre érvényes függvénytípus-paraméterezési szabályra:

```kotlin
private val addToQueue: (guildId: String, query: String) -> Flow<Resource<BotControlCommand>>
```

A függvénytípusban szereplő paraméternevek az olvashatóságot és az IDE támogatását segítik. A Kotlin függvénytípusú érték hívásakor ezekkel a nevekkel sem enged név szerinti argumentumátadást, ezért az `addToQueue(guildId, query)` hívás pozicionális marad. Ez a nyelvi korlát nem indok a deklaráció paraméterneveinek elhagyására.

### Blokkok, feltételek és hívásláncok

- Négy szóközös behúzást használj, tabulátor nélkül. A nyitó kapcsos zárójel a deklaráció vagy vezérlési szerkezet sorának végére kerüljön.
- Az utasításként használt `if` és `else` ágak, valamint a `for`, `while` és `do` ciklusok mindig kapjanak kapcsos zárójelet.
- Rövid, értéket előállító `if` kifejezés és egyetlen kifejezésből álló `when` ág maradhat egysoros. Több utasításból álló ág mindig blokkot kapjon.
- Az utasításként használt vezérlési blokkok előtt és után legyen üres sor. Közvetlenül blokkhatár mellett, illetve összetartozó `else if` és `else` ágak között nem kell külön üres sor.
- A 70 karakternél hosszabb feltételt logikai részek mentén tördeld; a `&&` és `||` operátor az új sor elejére kerüljön. A záró `)` és a nyitó `{` a feltétel utolsó sorának végén álljon.
- Több lépésből álló Flow-, kollekció-, Modifier- és builderláncnál az első hívás maradjon a kiinduló objektummal egy sorban, minden további hívás külön sorba, egy behúzási szinttel beljebb kerüljön. A több soros lambdák törzsét ehhez igazítsd.
- Ne használj felesleges pontosvesszőt. Enum deklarációban a tagokat követő további deklarációk elválasztásához szükséges pontosvessző maradjon meg.
- Az importokat rendezd, a használatlanokat távolítsd el. Új kódban használj explicit importokat a wildcard importok helyett; az ütközéseket szükség esetén alias oldja fel.
- A fájl végén egyetlen sorvége legyen, fölösleges üres sorok és sorvégi szóközök nélkül.

## Elnevezés és fájlszerkezet

- Az osztályok, interface-ek, objectek és típusálnevek neve `PascalCase`; a függvények, paraméterek és property-k neve `camelCase` legyen.
- A `Unit` visszatérésű Compose UI-függvények neve `PascalCase`. Értéket előállító segédfüggvények neve `camelCase` legyen.
- A nevek angolul, pontosan írják le az adott típus, függvény vagy érték konkrét jelentését és felelősségét. A modul meglévő fogalmait használd, de az általános vagy pontatlan neveket ne vedd át mintaként.
- A teljes projektben a változók, property-k, konstruktor- és függvényparaméterek, valamint lambda- és callback-paraméterek neve tükrözze, pontosan milyen értéket vagy függőséget tartalmaznak. A típus és a szerep alapján válassz teljes, beszédes `camelCase` nevet: például `AuthRepository` típushoz `authRepository`, ne csupán `repository` tartozzon.
- Ez minden más szerepkörre is érvényes: például egy konkrét ViewModel, use case, service, mapper vagy controller neve is azonosítsa a konkrét felelősségét. Ne használj olyan általános neveket, mint `viewModel`, `useCase`, `service`, `mapper` vagy `controller`, ha azok elhagyják az érték konkrét jelentését. A lambda-paramétereket is nevezd el beszédesen az általános `it` helyett.
- A rövid, általános elnevezés alól kizárólag a képernyőállapotot jelölő `uiState` és az egyszeri hatásokat jelölő `effect` kivétel, a hozzájuk tartozó `_uiState` és `_effect` backing property-kkel együtt. Ezeket nem kell feature-előtaggal ellátni, például `currentTrackUiState` névre átnevezni.
- A package neve kisbetűs, a mappaszerkezettel összhangban álló legyen. A saját Kotlin-kód gyökere `com.dc.melodiasmario`; a folytatás a modult és a funkciót tükrözze.
- Interface nevéhez ne adj automatikusan C#-os `I` előtagot. A szerepet leíró nevet használd; az implementáció kövesse a környező kód konvencióját.
- Egy fő publikus típust tegyél egy fájlba, a típus nevével egyező fájlnévvel. A szorosan összetartozó extensionök és top-level segédfüggvények leíró nevű közös fájlban maradhatnak.
- A privát beágyazott implementációs típusok és a szorosan összetartozó sealed hierarchia tagjai maradhatnak a befoglaló fájlban. Különálló adatmodellek külön fájlba kerüljenek.
- A nem változó referenciák `val` deklarációt kapjanak; `var` csak tényleges újraértékadás esetén legyen.
- A privát property-k általában `camelCase` nevűek. Publikus read-only állapot mögötti módosítható backing property lehet `_uiState` vagy `_effect`.
- Konstansok elnevezésekor kövesd a meglévő konstanscsoport mintáját; új csoportban `UPPER_SNAKE_CASE` neveket használj. Pusztán stílus miatt ne nevezd át a meglévő konstansokat.
- A láthatóság legyen a szükséges legszűkebb. Publikus és modulhatáron át használt API-knál a visszatérési típust explicit add meg.

## Modulok, rétegek és dependency injection

- A platformfüggetlen logika `commonMain` alá kerüljön. Android- és iOS-specifikus API-t a megfelelő `androidMain` vagy `iosMain` source setben használj; a közös határt szükség esetén interface vagy `expect`/`actual` adja.
- A domain use case-ek a felhasználási műveleteket koordinálják, a repository-szerződések az adatelérést írják le. A domain ne függjön UI-tól vagy konkrét hálózati implementációtól.
- A `core:data` implementálja a repository-kat és koordinálja az adatforrásokat. A HTTP, DTO és realtime szállítás a `core:network`, a helyi beállítástárolás a `core:datastore` felelőssége.
- A `core:model` a valóban megosztott modellek helye. A kizárólag lokális DTO-t, UI-modellt vagy SDK-függő típust ne költöztesd automatikusan ide.
- A `core:presentation` közös állapotkezelést, command handlereket, reducereket és prezentációs modelleket tartalmaz. Őrizd meg a Compose-tól, `core:common-ui`-tól és `core:domain`-től való függetlenségét.
- A megosztott Compose-komponensek és UI-erőforrások a `core:common-ui` modulba kerüljenek. A feature-specifikus prezentáció a `feature/<feature>/presentation`, a képernyők és komponensek a `feature/<feature>/ui` modulba tartoznak.
- A működést végző típusokat és az adatmodelleket felelősség szerint különítsd el; használd a meglévő `model`, `dto`, `mapper`, `usecase`, `repository` és más célzott package-eket. Ne hozz létre párhuzamos mappastruktúrát ugyanarra a szerepre.
- Használd a projekt meglévő Koin-alapú dependency injection megoldását és konstruktorinjektálását. Az új típust a megfelelő modulban vagy komponenskeresési hatókörben regisztráld.
- Csak ténylegesen használt függőséget injektálj. Üzleti logikában ne hozz létre közvetlenül HTTP-klienst, repository-t vagy platformszolgáltatást.
- Modulfüggőség módosításakor ellenőrizd az érintett `build.gradle.kts` fájlokat és a függőségi irányt; körkörös függőséget ne vezess be.
- Új függőséghez használd a meglévő version catalogot, ha az adott függőség ott kezelhető. Pusztán formázási feladathoz ne adj hozzá új csomagot.

## Állapotkezelés, coroutine-ok és hibakezelés

- Kövesd a feature meglévő `UiState`, `Event`, `Effect` és ViewModel felosztását. A tartós képernyőállapotot és az egyszeri navigációs vagy üzenetküldési hatásokat külön kezeld.
- A módosítható `MutableStateFlow` és `MutableSharedFlow` maradjon privát; kifelé csak olvasható felületet adj.
- Összetett állapotfrissítésnél használj `update`-et és immutábilis állapotmásolatot. A UI ne módosítsa közvetlenül a ViewModel állapotát.
- Aszinkron művelethez `suspend` függvényt és a meglévő Flow-szerződéseket használd. Ne vezess be C#-os `Task` vagy `CancellationToken` mintát.
- A coroutine a tulajdonos életciklusához kötött scope-ban fusson, például `viewModelScope`-ban. `GlobalScope`-ot ne használj.
- A coroutine-megszakítást őrizd meg. Általános kivételkezelés vagy `runCatching` ne nyelje el a `CancellationException`-t.
- Blokkoló I/O-t ne futtass a UI-szálon. A dispatcher kiválasztása az adott I/O-határ felelőssége; tesztelhető absztrakciót a meglévő projektminta szerint adj.
- Kövesd a meglévő `Resource` és hibamodell szerződéseket. A loading állapot és command lock siker, hiba és megszakítás esetén is konzisztens maradjon.
- Ne használj `!!`-t bizonyítható szükség nélkül. A hiányzó vagy érvénytelen adatot a szerződésnek megfelelően kezeld; hibát ne rejts el indokolatlan alapértékkel.
- Naplóba ne kerüljön token, jelszó vagy más érzékeny adat. A meglévő naplózási megoldást használd.

## Compose UI-konvenciók

- A `Route` kezelje a ViewModel bekötését, az állapot megfigyelését és az effectek fogadását. A `Screen` és újrafelhasználható komponensek állapotot és callbackeket kapjanak.
- A komponensek legyenek előnézetben megjeleníthetők és tesztelhetők élő hálózat, repository és platformszolgáltatás nélkül.
- Ahol indokolt, adj `modifier: Modifier = Modifier` paramétert, és a kapott modifiert a komponens gyökérelemére alkalmazd. Új API-nál az első opcionális paraméter legyen a modifier.
- Mellékhatást ne indíts közvetlenül a composable törzséből. Használd a megfelelő effect API-t, a tényleges függőségeket tükröző kulcsokkal és szükség esetén takarítással.
- A felhasználó számára látható szöveg Compose-erőforrásból származzon. Új vagy módosított kulcsnál tartsd összhangban az érintett nyelvi fájlokat; duplikált kulcsot ne hozz létre.
- A UI színeit a `MaterialTheme.colorScheme` szemantikus szerepeiből vedd, a méretezéshez és tipográfiához a projekt meglévő megoldásait használd. Fix dekoratív artwork-színek maradhatnak, ha szándékosak.
- Interaktív elemeknél őrizd meg a hozzáférhetőségi szemantikát és a megfelelő leírásokat. A dekoratív képekhez ne adj fölösleges felolvasandó szöveget.
- Megosztott komponens módosításakor ellenőrizd a hívási helyeket és az érintett képernyők állapotait, például a loading, üres, hiba és tartalom állapotot.

## KDoc és kódkommentek

- Minden új vagy érdemben módosított publikus osztály, interface és publikus szerződéshez tartozó függvény vagy property kapjon KDoc-dokumentációt. A szerződés részeként használt `internal` API-kat is dokumentáld.
- A KDoc a célt, a szerződést és a nem nyilvánvaló viselkedést írja le. Indokolt esetben térj ki a paraméterek jelentésére, mértékegységére, indexelésére, hibákra és mellékhatásokra.
- Privát segédfüggvényhez és mezőhöz csak akkor írj kommentet, ha a működés vagy döntés magyarázatra szorul. A kódot pusztán megismétlő kommentet kerüld.
- A KDoc a deklaráció annotációi elé, a deklarációval azonos behúzással kerüljön. Előtte legyen üres sor, kivéve közvetlenül a fájl vagy blokk elején.
- Override esetén ne másold le szükségtelenül az örökölt szerződést; az eltérő vagy kiegészítő viselkedést dokumentáld.
- A kód dokumentációja angol nyelvű legyen. A Kotlin-dokumentációhoz KDoc-ot használj, XML-komment helyett.

## Tesztelés és befejezés előtti ellenőrzés

### Csak tesztvázak készítése

- A teljes MobileApp projektben új tesztekhez kizárólag vázakat készíts: tesztosztályt és benne a tervezett teszteseteket. A tesztlogikát ne implementáld, kivéve ha a felhasználó ezt külön kéri.
- Minden teszteset kapjon a használt tesztkeretrendszernek megfelelő annotációt, például `@Test`, és beszédes angol nevet, amely a helyzetet és az elvárt viselkedést írja le.
- Minden új teszteset törzse kizárólag `throw NotImplementedError()` legyen. Ne hagyj üres vagy csak TODO-kommentet tartalmazó, látszólag sikeres tesztet.
- A tesztvázba ne írj assertet, mockot, fake-et, fixture-t, tesztadat-előkészítést vagy coroutine-tesztlogikát. A váz miatt ne vezess be új tesztfüggőséget.
- A tesztosztály neve a vizsgált típust vagy folyamatot tükrözze, `Test` utótaggal, és azonos nevű fájlba kerüljön.
- A tesztesetek tervezésekor a megfigyelhető viselkedésből indulj ki. A releváns sikeres, hibás és határeseteket nevezd meg; az implementáció sorait ne másold tesztesetekké.
- Nem szükséges minden típushoz minden tesztszinten külön tesztosztály. A tiszta üzleti döntéshez unit, adapterhez vagy adatforráshoz indokolt esetben integration, fontos felhasználói folyamathoz UI- vagy E2E-tesztváz készüljön.
- A platformfüggetlen tesztváz `commonTest`, a platformhoz kötött váz a megfelelő host-, device- vagy iOS-teszt source setbe kerüljön.
- A meglévő implementált teszteket őrizd meg; ne alakítsd őket helykitöltővé. A meglévő tesztvázakat külön kérés nélkül ne implementáld.

A tesztváz kötelező mintája, egy meglévő projektbeli tesztosztály és teszteset nevével szemléltetve:

```kotlin
import kotlin.test.Test

class LoginViewModelTest {

    @Test
    fun startStatusPollingUpdatesStateWithOnlineStatus() {
        throw NotImplementedError()
    }
}
```

### Ellenőrzés és eredmények közlése

- Kódmódosításnál ellenőrizd az érintett modul fordíthatóságát, és futtasd a változáshoz kapcsolódó meglévő, implementált teszteket, ahol lehetséges. A Gradle task nevét a projekt konfigurációjából vagy tasklistájából ellenőrizd.
- A tesztváz csak a tervezett teszteseteket rögzíti, működési lefedettséget nem ad. A `NotImplementedError` miatt futtatva hibát jelez; ezt helykitöltő tesztesetként jelentsd, és pusztán a sikeres futás kedvéért ne implementáld vagy ürítsd ki.
- Több modult vagy az alkalmazás összeállítását érintő módosításnál az Android build ellenőrzése: `.\gradlew.bat :androidApp:assembleDebug`.
- iOS-ellenőrzést megfelelő macOS/Xcode környezetben végezz. Más platform sikeres buildjét ne tekintsd iOS-validációnak.
- Dokumentációs vagy kizárólag formázási módosításnál arányos ellenőrzést végezz; működést nem érintő változáshoz ne készíts mesterséges tesztvázat.
- Befejezés előtt ellenőrizd a diffet: név szerinti argumentumok, záró vesszők hiánya, importok, formázás és a feladathoz tartozó változtatások.
- A végén jelezd, mi változott, milyen ellenőrzés futott, és mi maradt ellenőrizetlen. A tesztvázakat, ignorált teszteket és sikeresen lefutott implementált teszteket egyértelműen különböztesd meg.

## README dokumentációs szabályok

### Általános szabályok

- A README-k angol nyelvű, GitHub-kompatibilis Markdown dokumentumok legyenek.
- Kizárólag a ténylegesen létező funkcionalitást és az ellenőrzött forráskódot dokumentáld. Nem létező típust, metódust, taskot vagy működést ne találj ki.
- README létrehozása vagy módosítása előtt vizsgáld meg az érintett forrásokat és konfigurációt.
- A meglévő, helyes dokumentációt őrizd meg. Csak az érintett szekciókat frissítsd; a teljes README-t ne írd újra szükségtelenül.
- Ne hozz létre üres, nem releváns szekciókat. Kerüld az emojikat és a fölösleges díszítést.

### Kötelező szerkezet, ahol releváns

- `# MappaNeve`: a könyvtár neve, alatta 1–2 mondat a céljáról.
- `## Overview`: felelősségek és fő képességek.
- `## Subfolders`: releváns almappák külön `### mappanév/` szekcióban, rövid leírással, `**Files:**` és `**Purpose:**` mezőkkel. A részletes leírásokat `---` válassza el.
- Komponensek logikai csoportjai `##`, konkrét típusok `###` címsor alatt. Szükség szerint `**Purpose:**`, `**Properties:**`, `**Methods:**`, `**Usage:**`, `**Validation Checks:**` és `**Benefits:**` mezőket használj.
- `## Related Components`: kapcsolódó modulok, típusok és könyvtárak, kapcsolatuk rövid magyarázatával.
- Moduloknál dokumentáld a releváns függőségi irányt és source seteket; platformkorlátot vagy szükséges konfigurációt akkor írj le, ha az ténylegesen érinti a használatot.

### Példák és hivatkozások

- Minden kódblokk kapjon nyelvazonosítót: Kotlinhoz `kotlin`, PowerShellhez `powershell`, XML-hez `xml`.
- A példák rövidek legyenek, valódi projektbeli API-kra épüljenek, és kövessék a név szerinti argumentumátadás és a záró vessző tiltásának szabályát.
- Fájlnevekhez, package-ekhez és azonosítókhoz inline kódformázást, fájlhivatkozásokhoz lehetőleg relatív Markdown-linket használj.
- Build- és tesztparancsnál írd le, mit ellenőriz a parancs; a sikeres fordítás nem bizonyítja a működési tesztek meglétét vagy sikerét.

### Automatikus karbantartás

1. Minden módosításnál ellenőrizd az érintett mappa és a mobilprojekt gyökér-README-jének aktualitását.
2. Működés, publikus API, konfiguráció, fejlesztési konvenció vagy struktúra változásakor frissítsd az érintett részeket.
3. Új, érdemi felelősséget hordozó mappához készíts README-t, ha annak használata vagy szerepe dokumentálást igényel.
4. Almappa vagy típus hozzáadásakor, átnevezésekor és törlésekor frissítsd a kapcsolódó felsorolásokat és hivatkozásokat a szülő dokumentációban is.
5. Ellenőrizd, hogy a dokumentált szignatúrák, példák és parancsok megfelelnek-e az aktuális implementációnak.
6. A dokumentáció frissítése a feladat része; ahol a változás nem érinti a leírtakat, ne készíts fölösleges dokumentációs módosítást.
