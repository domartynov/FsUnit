﻿module FsUnit.MbUnit

open System
open MbUnit.Framework
open NHamcrest
open NHamcrest.Core

let should (f : 'a -> #IMatcher<obj>) x (y : obj) =
    let c = f x
    let y =
        match y with
        | :? (unit -> unit) as assertFunc -> box assertFunc
        | _ -> y
    Assert.That(y, c)

let equal x = Is.EqualTo<obj> x

//TODO: Look into a better way of doing this.
let equalWithin (tolerance:obj) (expected:obj) = new CustomMatcher<obj>("Is equalWithin", 
                                                     fun x -> let actualParsed, actual = Double.TryParse(string x)
                                                              let expectedParsed, expect = Double.TryParse(string expected)
                                                              let toleranceParsed, tol = Double.TryParse(string tolerance)
                                                              if actualParsed && expectedParsed && toleranceParsed then
                                                                  abs(actual - expect) <= tol
                                                              else false )

let not (x:obj) = match box x with
                  | :? IMatcher<obj> as matcher -> Is.Not<obj>(matcher)
                  |  x -> Is.Not<obj>(Is.EqualTo<obj>(x))

let throw (t:Type) = new CustomMatcher<obj>("Should throw exception", 
                         fun f -> match f with
                                  | :? (unit -> unit) as testFunc -> 
                                      try
                                        testFunc() 
                                        false
                                      with
                                      | ex -> if ex.GetType() = t then true else false
                                  | _ -> false )

let be = id

let Null = Is.Null()

let EmptyString = new CustomMatcher<obj>("A non empty string", fun s -> (string s).Trim() = "")

let NullOrEmptyString = new CustomMatcher<obj>("A non empty or not null string", fun s -> String.IsNullOrEmpty(unbox s))

let True = new CustomMatcher<obj>("Is true", fun b -> unbox b = true)

let False = new CustomMatcher<obj>("Is false", fun b -> unbox b = false)

let sameAs x = Is.SameAs<obj>(x)

let greaterThan (expected:obj) = new CustomMatcher<obj>("Is greater than the provided value", 
                                     fun actual -> (unbox actual :> IComparable).CompareTo(unbox expected) > 0)

let greaterThanOrEqualTo (expected:obj) = new CustomMatcher<obj>("Is greater than or equal to the provided value", 
                                              fun actual -> (unbox actual :> IComparable).CompareTo(unbox expected) >= 0)

let lessThan (expected:obj) = new CustomMatcher<obj>("Is less than the provided value", 
                                    fun actual -> (unbox actual :> IComparable).CompareTo(unbox expected) < 0)

let lessThanOrEqualTo (expected:obj) = new CustomMatcher<obj>("Is less than or equal to the provided value", 
                                           fun actual -> (unbox actual :> IComparable).CompareTo(unbox expected) <= 0)

let endWith (input:string) = new CustomMatcher<obj>("A string ends with a specified value", fun s -> (string s).EndsWith input)

let startWith (input:string) = new CustomMatcher<obj>("A string starts with a specified value", fun s -> (string s).StartsWith input)

let ofExactType<'a> = new CustomMatcher<obj>("Is exact type", fun x -> (unbox x).GetType() = typeof<'a>)

let contain x = new CustomMatcher<obj>("Collection has item", 
                          fun c -> match c with
                                   | :? list<_> as l -> l |> List.exists(fun i -> i = x)
                                   | :? array<_> as a -> a |> Array.exists(fun i -> i = x)
                                   | :? seq<_> as s -> s |> Seq.exists(fun i -> i = x)
                                   | _ -> false)

// haveLength, haveCount, Empty, and shouldFail are not implemented for MbUnit