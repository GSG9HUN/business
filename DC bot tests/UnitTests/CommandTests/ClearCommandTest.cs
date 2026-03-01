namespace DC_bot_tests.UnitTests.CommandTests;

// TODO: A ClearCommand-hoz nincsenek tesztek megírva. Az alábbi esetekre kellene teszteket írni:
//   - ExecuteAsync_UserIsBot_ShouldDoNothing: bot felhasználó hívja a parancsot -> semmit sem csinál
//   - ExecuteAsync_ValidationFails_ShouldSendError: validáció sikertelen (pl. felhasználó nincs hangcsatornában) ->
//     validációs hiba üzenet küldése, SetQueue nem hívódik meg
//   - ExecuteAsync_ValidUser_ShouldClearQueue: érvényes felhasználó -> SetQueue meghívódik üres queue-val,
//     sikeres válasz üzenet kerül kiküldésre
//   - Command_Name_And_Description_ShouldReturnCorrectValue: a Name "clear", a Description helyes értéket ad vissza
public class ClearCommandTest
{
    // TODO: implement test cases (see comments above).
}
