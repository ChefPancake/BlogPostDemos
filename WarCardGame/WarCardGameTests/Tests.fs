namespace WarCardGameTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open WarCardGame.Common
open WarCardGame.Components
open WarCardGame.GameSetup
open WarCardGame.GameFunctions

[<TestClass>]
type TestClass () =
    
    let unwrapOrFailWith buildErrMessage res =
        match res with
        | Ok x -> x
        | Error err -> failwith (buildErrMessage err)


    [<TestMethod>]
    member this.chain () =
        let addOne = (+) 1
        let expected = 5

        let lastOfFive =
            chain addOne 0
            |> Seq.take expected
            |> Seq.last

        Assert.AreEqual(expected, lastOfFive)

    [<TestMethod>]
    member this.takeWhileIncl () =
        let addOne = (+) 1
        let expected = 5
        let isLessThanExpected x =
            x < expected

        let lastOfFive =
            chain addOne 0
            |> takeWhileIncl isLessThanExpected
            |> Seq.last

        Assert.AreEqual(expected, lastOfFive)


    [<TestMethod>]
    member this.runFullGameTwoPlayers () =
        let init = 
            trySetupGame 2
            |> unwrapOrFailWith ((+) "Failed to set up game: ")
        let playerChoice _ cards =
            cards
        let result = 
            resolveWarGame
                playerChoice
                init

        match result with
        | GameState.CompletedGame winners ->
            Console.WriteLine("Winners!: " + String.Join(" ", List.toArray winners))
        | GameState.RunningGame x ->
            Assert.Fail "unable to resolve game"



