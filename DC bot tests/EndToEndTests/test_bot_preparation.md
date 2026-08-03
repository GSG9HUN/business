# E2E test bot elokeszitesi terv

Ez a dokumentum azt irja le, hogyan keszitenem elo a Discord test botot az E2E tesztek megirasahoz.

A legfontosabb dontes: nem vezetnek be kulon masodik Discord klienst. A bot sajat `DiscordClient` peldanya kuldi be a teszt parancsot a teszt szoveges csatornaba, majd ugyanennek a kliensnek a gateway esemenyein keresztul fut le a `MessageCreated` pipeline. Ez csak akkor mukodik helyesen, ha a bot test modban indul, mert production modban a bot szerzotol erkezo uzeneteket a command handler es a user validacio is kiszuri.

## Celallapot

Az E2E teszt elokeszites vegen ezek legyenek keszen:

1. Egy dedikalt Discord test guild.
2. Egy dedikalt Discord test bot, amely bent van ebben a guildben.
3. Egy szoveges teszt csatorna, ahova a bot irhat es ahonnan uzeneteket olvashat.
4. Music flow-khoz egy voice teszt csatorna es elerheto Lavalink.
5. Egyetlen E2E fixture, amely elinditja a valos botot test modban.
6. Egy helper, amely a bot kliensen keresztul kuld parancsot sajat maganak.
7. Egy helper, amely az elkuldott parancs utan varja a bot valaszat.

## Miert eleg egyetlen bot kliens?

A jelenlegi kodban a `CommandHandlerService` ezt csinalja:

1. `MessageCreated` esemenyt kap DSharpPlus-bol.
2. Megnezi, hogy regisztralva van-e a handler.
3. Production modban eldobja a bot szerzotol erkezo uzenetet.
4. Test modban ezt nem dobja el.
5. Prefix alapjan kivalasztja a commandot.
6. `DiscordMessageWrapperFactory`-n keresztul atadja a valos Discord uzenetet a commandnak.

Ezert E2E-ben a bot altal kuldott `!ping` vagy `!help` uzenet is vegig tud menni ugyanazon a valos Discord API + gateway + command handler utvonalon, mint egy emberi user uzenete. Emiatt a `DISCORD_E2E_SENDER_TOKEN` es a `DISCORD_TEST_SENDER_TOKEN` iranyt nem vinnem tovabb.

Ami ezzel nem lesz lefedve: az, hogy egy kulon emberi vagy masodik bot user voice state-je es jogosultsagai pontosan hogyan viselkednek. Ez kulon kockazat a music flow-knal, mert a `JoinCommand` es a `PlayCommand` az uzenet szerzojenek voice channeljet keresi.

## 1. Discord test kornyezet

Eloszor kell egy dedikalt teszt guild.

Miert kell: az E2E teszt valos Discord uzeneteket fog kuldeni. Nem jo otlet fejlesztoi vagy production szerveren futtatni, mert a tesztek zajt, reakciokat, csatlakozasokat es cleanup feladatokat okoznak.

Letrehozando elemek:

1. Test guild, peldaul `dc-bot-e2e`.
2. Szoveges csatorna, peldaul `#e2e-text`.
3. Voice csatorna music flow-khoz, peldaul `E2E Voice`.
4. A test bot meghivasa a guildbe.

Minimalis bot jogosultsagok:

1. `View Channel`
2. `Send Messages`
3. `Read Message History`
4. `Add Reactions`, ha reaction handler E2E is lesz
5. `Connect` es `Speak`, ha Lavalink/music E2E is lesz
6. Slash command flow-khoz application command scope

Discord Developer Portal oldalon engedelyezendo:

1. Message Content Intent
2. Server Members Intent, ha member/voice validaciohoz kell
3. Presence intent csak akkor, ha kesobb konkretan hasznaljuk

Miert kell a Message Content Intent: a text command pipeline prefixbol dolgozik, tehat a `MessageCreatedEventArgs.Message.Content` erteke nelkul a command handler nem tudja kiolvasni a `!command` szoveget.

## 2. Kornyezeti valtozok

Az E2E konfiguracio ne hardcode-oljon tokent vagy ID-t. Ezek kornyezeti valtozokbol jojjenek.

Kotelezo alap valtozok:

```text
DISCORD_TOKEN=<test bot token>
DISCORD_TEST_GUILD_ID=<test guild id>
DISCORD_TEST_CHANNEL_ID=<test text channel id>
```

Music flow-khoz:

```text
DISCORD_TEST_VOICE_CHANNEL_ID=<test voice channel id>
LAVALINK_HOSTNAME=<hostname>
LAVALINK_PORT=<port, default: 2333>
LAVALINK_PASSWORD=<password, lehet ures is>
LAVALINK_SECURED=<true vagy false>
```

Opcionalis:

```text
BOT_PREFIX=!
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=dc_bot
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
```

Nem kell:

```text
DISCORD_E2E_SENDER_TOKEN
DISCORD_TEST_SENDER_TOKEN
```

Miert nem kell sender token: a teszt parancsot a fixture altal elinditott bot kliens kuldi be. A command handler test modban nem szuri ki a sajat bot uzenetet, ezert nincs technikai igeny masodik kliensre.

## 3. `EndToEndTestConfiguration.cs` felelossege

Az `EndToEndTestConfiguration.cs` ne inditson botot es ne epitsen service providert statikus inicializalas kozben. Csak konfiguraciot toltson be es validaljon.

Javasolt felelossegek:

1. `BotConfigurationLoader.LoadFromEnvironment(...)` meghivasa.
2. `DISCORD_TEST_GUILD_ID` beolvasasa es `ulong`-ga parsolasa.
3. `DISCORD_TEST_CHANNEL_ID` beolvasasa es `ulong`-ga parsolasa.
4. `DISCORD_TEST_VOICE_CHANNEL_ID` opcionalis beolvasasa.
5. Hianyzo vagy hibas valtozoknal `null` visszaadasa.
6. A hiany okat kiirni a teszt outputra vagy egy `MissingReason` stringbe.

Miert igy: a konfiguracio csak adat. Ha mar itt service provider epul vagy Discord kliens indul, akkor a teszt setup nehezen kontrollalhato lesz, a hianyzo kulso kornyezet pedig compile/runtime hibakhoz vezethet egyszeru skip helyett.

Javasolt forma:

```csharp
internal sealed record EndToEndTestConfiguration(
    BotRuntimeSettings RuntimeSettings,
    ulong GuildId,
    ulong TextChannelId,
    ulong? VoiceChannelId)
{
    public static EndToEndTestConfiguration? Load(TextWriter? output = null)
    {
        output ??= Console.Out;

        var runtimeSettings = BotConfigurationLoader.LoadFromEnvironment(output);
        if (runtimeSettings is null)
        {
            return null;
        }

        if (!TryReadUInt64("DISCORD_TEST_GUILD_ID", output, out var guildId))
        {
            return null;
        }

        if (!TryReadUInt64("DISCORD_TEST_CHANNEL_ID", output, out var textChannelId))
        {
            return null;
        }

        var voiceChannelId = TryReadOptionalUInt64("DISCORD_TEST_VOICE_CHANNEL_ID", output);
        return new EndToEndTestConfiguration(runtimeSettings, guildId, textChannelId, voiceChannelId);
    }
}
```

A lenyeg nem a pontos kodreszlet, hanem a szetvalasztas: konfiguracio betoltes itt, bot inditas a fixture-ben.

## 4. E2E fixture felelossege

A `DiscordE2EClientFixture` legyen az egyetlen hely, amely valos Discord klienst indit.

Lepesek:

1. Meghivja az `EndToEndTestConfiguration.Load()` metodust.
2. Ha nincs konfiguracio, beallit egy `MissingReason` erteket es nem indit klienst.
3. Letrehozza a production DI containert `BotServiceProviderFactory.Create(configuration.RuntimeSettings, isTestEnvironment: true)` hivassal.
4. Meghivja a `BotHandlerRegistrar.RegisterHandlers(_services)` metodust.
5. Elinditja a botot `BotService.StartAsync(isTestEnvironment: true)` hivassal.
6. Megvarja, hogy `DiscordClient.CurrentUser` ne legyen `null`.
7. Betolti a guildet es a szoveges csatornat az ID-k alapjan.
8. Music tesztekhez opcionalisan betolti a voice csatornat.
9. Dispose alatt lecsatlakoztatja a klienst es dispose-olja a service providert.

Miert kell a `isTestEnvironment: true`: ez kapcsolja at a command handler es a `ValidationService` bot user szureset. Enelkul a bot sajat uzenetei nem futnanak le commandkent.

Miert kell a handler regisztracio: a DSharpPlus event handler regisztralva van a DI-ben, de a `CommandHandlerService` csak akkor dolgozza fel az eventet, ha a sajat `RegisterHandler(...)` metodusa mar lefutott.

Miert kell a `CurrentUser` wait: a Discord gateway kapcsolat aszinkron. A teszt csak akkor kuldjon uzenetet, ha a kliens mar kesz allapotban van.

## 5. Egy klienses parancskuldo helper

A fixture kapjon egy egyszeru helper metodust, vagy kulon helper osztalyt.

Javasolt mukodes:

1. Ellenorzi, hogy a fixture konfiguralt-e.
2. Lekeri a teszt szoveges csatornat.
3. Osszerakja a parancsot a konfiguralt prefixbol es a command nevbol.
4. Bekuldi a parancsot ugyanazzal a `DiscordClient` peldannyal.
5. Visszaadja az elkuldott `DiscordMessage` objektumot anchornek.

Pelda API:

```csharp
public Task<DiscordMessage> SendBotCommandAsync(string commandName, string? arguments = null)
```

Pelda command text:

```text
!ping
!help
!language hu
```

Miert kell visszaadni az elkuldott uzenetet: a kovetkezo varakozasnak tudnia kell, mi utan kell keresni a bot valaszat. Igy regi uzenetek vagy elozo teszt futasok nem okoznak hamis pozitiv eredmenyt.

## 6. Valasz varasa

A teszt ne fix `Task.Delay(...)` utan ellenorizzen. Hasznaljuk vagy bovitsuk a mar letezo `DiscordMessageWaiter` helpert.

Javasolt pattern:

1. A command helper visszaadja a bekuldott command message-t.
2. A teszt `DiscordMessageWaiter.WaitForMessageAfterAsync(...)` hivassal csak az ez utani uzenetekben keres.
3. A predicate konkret legyen: author, content vagy embed alapjan ellenorizzen.
4. Timeout legyen rovid, de ne tul agressziv. A jelenlegi default 10 masodperc jo kiindulas.

Pelda:

```csharp
var commandMessage = await fixture.SendBotCommandAsync("ping");

var response = await DiscordMessageWaiter.WaitForMessageAfterAsync(
    fixture.TextChannel,
    commandMessage.Id,
    message => message.Author.Id == fixture.Client.CurrentUser.Id &&
               MessageLooksLikePingResponse(message),
    "ping response from the E2E bot");
```

Miert kell predicate: a Discord csatorna kulso allapot. Ha csak az elso uzenetet vesszuk, mas teszt vagy manualis uzenet megzavarhatja az eredmenyt.

## 7. Elso E2E smoke teszt

Az elso teszt ne music flow legyen. Legyen egy minimalis text command, peldaul `!ping`.

Javasolt teszt:

1. Fixture indul.
2. Ha a fixture nincs konfiguralva, a teszt outputra kiirja az okot es visszater.
3. A bot bekuldi sajat maganak: `!ping`.
4. A teszt megvarja a ping valaszt.
5. A teszt ellenorzi, hogy a valasz a bot command pipeline-bol jott.

Miert ezzel kell kezdeni: ez igazolja a teljes alap E2E lancot Lavalink es voice state nelkul:

```text
Discord API send -> Discord gateway MessageCreated -> CommandHandlerService -> command registry -> PingCommand -> ResponseBuilder -> Discord API response
```

Ha ez nem stabil, akkor a music E2E-k csak zajosabb hibakat fognak adni.

## 8. Music es voice flow elokeszites

A music parancsoknal kulon figyelni kell arra, hogy egy klienses E2E-ben az uzenet szerzoje maga a bot.

A jelenlegi validacio ezt nezi:

1. `message.Author`
2. `message.Channel.Guild.GetMemberAsync(author.Id)`
3. `member.VoiceState.Channel`

Ez azt jelenti, hogy `!join` vagy `!play` elott a bot membernek kell voice channelben lennie, vagy a teszt modnak kell explicit voice channel override-ot adnia.

Ket jarhato irany:

1. A fixture elokesziti a bot voice state-et a teszt voice csatornaban, es csak ezutan kuldi be a music parancsot.
2. Test-only helperrel a music E2E-k nem a valos human voice state-et tesztelik, hanem a command handler pipeline-t es a Lavalink csatlakozasi utvonalat kontrollalt voice channel ID-val.

A masodik kliens tovabbra sem kell. A fontos az, hogy a teszt setup egyertelmuen kezelje: a sajat uzenetes flow-ban a bot a szerzo, tehat a voice precondition nem ugyanaz, mint egy manualis Discord kliensbol irt emberi `!join`.

## 9. Cleanup szabalyok

Minden E2E futas hagyjon maga utan tiszta allapotot.

Javasolt cleanup:

1. A teszt altal kuldott command message torlese, ha a botnak van ra joga.
2. A bot valaszuzeneteinek torlese, ha konnyen azonosithatok.
3. Lavalink player leallitasa music teszt utan.
4. Bot voice disconnect music teszt utan.
5. Queue/current track adat torlese vagy izolalt test guild adat hasznalata.

Miert kell: az E2E csatorna allapota kulso es tartos. Ha a tesztek szemetet hagynak, a kovetkezo futasban a waiter predicate rossz uzenetet talalhat.

## 10. xUnit futtatas es gating

Az E2E tesztek mar `Trait("Category", "E2E")` jelolest hasznalnak. Ezt megtartanam.

Futtatas:

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=E2E"
```

Javasolt szabaly:

1. Hianyzo Discord/Lavalink konfiguracio ne buktassa a normal unit/integration teszt futast.
2. E2E futasnal a teszt irja ki, mi hianyzik.
3. CI-ban csak akkor fusson E2E, ha a secretok es kulso szolgaltatasok be vannak allitva.
4. E2E collection legyen szerializalva, hogy ne induljon egyszerre tobb valos Discord kliens ugyanazzal a tokennel.

Miert kell szerializalas: ugyanaz a bot token, ugyanaz a csatorna es ugyanaz a Lavalink player kulso, megosztott eroforras. Parhuzamos futasnal flaky hibak es rate limit jelenhet meg.

## 11. Megvalositasi sorrend

Ezt a sorrendet kovetnem:

1. Rendbe rakni az `EndToEndTestConfiguration.cs` fajlt, hogy csak konfiguraciot toltson be.
2. Eltavolitani vagy nem hasznalni a sender token konstansokat.
3. Kiegesziteni a `DiscordE2EClientFixture`-t a `MissingReason`, `RuntimeSettings`, `Guild`, `TextChannel` es opcionalis `VoiceChannel` propertykkel.
4. Biztositani, hogy a fixture mindig `isTestEnvironment: true` modban epitse es inditsa a botot.
5. Hozzaadni a self-message command helper metodust.
6. Hozzaadni vagy boviteni a valasz varakozo helpert.
7. Megirni az elso `!ping` E2E smoke tesztet.
8. Csak ezutan menni tovabb `!help`, `!language`, playlist es music iranyba.
9. Music flow elott kulon donteni a voice setup kezeleserol.
10. Cleanupot minden E2E teszt reszeve tenni.

## 12. Elfogadasi kriteriumok

A test bot elokeszites akkor tekintheto kesznek, ha:

1. Nincs szukseg masodik Discord tokenre.
2. Egyetlen fixture inditja es allitja le a valos bot klienst.
3. A fixture test modban inditja a botot.
4. A bot tud sajat maganak `!ping` parancsot kuldeni a teszt csatornaba.
5. A command handler feldolgozza a bot sajat uzenetet.
6. A teszt meg tudja varni es ellenorizni a bot valaszat.
7. Hianyzo kornyezetnel a teszt ertheto okkal no-op lesz.
8. Music flow-khoz dokumentaltan kezelve van a bot-author voice state kerdese.
