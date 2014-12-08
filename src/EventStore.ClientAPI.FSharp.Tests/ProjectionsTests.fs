﻿module Tests.ProjectionsTests

open System
open System.Net
open System.Reflection
open System.Threading
open System.Threading.Tasks
open System.IO

open Fuchu

open TestUtilities

open LifeOfAProgrammer

open EventStore.ClientAPI
open EventStore.ClientAPI.Exceptions
open EventStore.ClientAPI.AsyncHelpers
open EventStore.ClientAPI.SystemData
open EventStore.ClientAPI.Common.Log

open Intelliplan.JsonNet

let run_t (t : Task) = t |> Async.AwaitTask |> Async.RunSynchronously

[<Tests>]
let roundtrip_tests =
  let proj_ctx =
    { logger  = ConsoleLogger()
      ep      = new IPEndPoint(IPAddress.Loopback, 2113)
      timeout = TimeSpan.FromSeconds 8.
      creds   = UserCredentials ("admin", "changeit") }

  let eventstore_impl = with_embedded_es

  testList "read your writes" [
    testCase "assumption: expected URN" <| fun _ ->
      let n = TypeNaming.nameObj MetAYakAndShavedIt
      Assert.Equal("expected name", "urn:Tests:LifeOfAProgrammer_Evt|MetAYakAndShavedIt", n)

    testCase "initialisation: can add read model" <| fun _ ->
      eventstore_impl <| fun _ ->
        Projections.wait_for_init proj_ctx <| fun _ -> async {
            do! Projections.ensure_continuous proj_ctx "EventCounting" (resource "EventCounting.js")
            do! Projections.ensure_continuous proj_ctx "IsHeAManagerYet" (resource "IsHeAManagerYet.js")
          }
        |> Async.RunSynchronously

//    testCase "cmds: can execute against Aggregate" <| fun _ ->
//      with_connection eventstore_impl <| fun conn ->
//        "programmers-1" |> Projections.delete proj_ctx |> Async.RunSynchronously
//        let evts =
//          CodeLikeHell
//          |> LifeOfAProgrammer.write conn "programmers-1" NoStream
//          |> Async.RunSynchronously
//
//        match evts with
//        | [_; MetAYakAndShavedIt] -> ()
//        | es -> Tests.failtest "got %A, expected hairy Yak" es
//
//        let _ =
//          PublishNuget
//          |> LifeOfAProgrammer.write conn "programmers-1" (Specific 2u)
//          |> Async.RunSynchronously
//        ()
//
//    testCase "projections: init/update/ensure" <| fun _ ->
//      let ensure_projections () =
//        Projections.wait_for_init proj_ctx <| fun _ -> async {
//            do! ("EventCounting", resource "EventCounting.js") ||> Projections.ensure_continuous proj_ctx
//            do! ("IsHeAManagerYet", resource "IsHeAManagerYet.js") ||> Projections.ensure_continuous proj_ctx
//          }
//        |> Async.RunSynchronously
//
//      let write_code_command conn =
//        CodeLikeHell
//        |> LifeOfAProgrammer.write conn "programmers-2" NoStream
//        |> Async.RunSynchronously
//        |> ignore
//
//      with_connection eventstore_impl <| fun conn ->
//        ensure_projections ()
//        write_code_command conn
//        let state = "EventCounting" |> Projections.get_state proj_ctx |> Async.RunSynchronously
//        printfn "state: %A" state
    ]