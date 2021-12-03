namespace WarCardGame
module Common =
    let tupleFirst (x, _) = x
    let tupleMapFirst f (x, y) = (f x, y)
    let tupleSecond (_, x) = x
    let tupleMapSecond f (x, y) = (x, f y)
    let tupleApply f (x, y) = f x y
    let tupleListSplit (items: ('a * 'b) list): ('a list * 'b list) = 
        let firsts = List.map tupleFirst items
        let seconds = List.map tupleSecond items
        (firsts, seconds)

    let rec chain (f: 'T -> 'T) (init: 'T): seq<'T> =
        seq {
            let next = f init
            yield next
            yield! chain f next
        }

    let takeWhileIncl 
            (predicate: 'T -> bool) 
            (items: seq<'T>)
            : seq<'T> =
        let head = Seq.head items
        Seq.pairwise items
        |> Seq.takeWhile (tupleFirst >> predicate)
        |> Seq.map tupleSecond
        |> Seq.append [head]

    type NonEmptyList<'T> = {
        Head: 'T
        Tail: 'T list
    }
    module NonEmptyList =
        let private fromListUnsafe l = {
            NonEmptyList.Head = List.head l
            NonEmptyList.Tail = List.tail l
        }
            
        let tryCreate items = 
            List.tryHead items
            |> Option.map
                (fun h -> { 
                    NonEmptyList.Head = h
                    NonEmptyList.Tail = List.tail items
                })
        let toList nonEmptyList =
            [nonEmptyList.Head] @ nonEmptyList.Tail

        let map f nonEmptyList = {
                NonEmptyList.Head = f nonEmptyList.Head
                NonEmptyList.Tail = List.map f nonEmptyList.Tail
            }

        let max projection nonEmptyList =
            toList nonEmptyList
            |> List.map projection
            |> List.max

        let maxBy projection nonEmptyList =
            toList nonEmptyList
            |> List.maxBy projection

        let sortBy keySelect nonEmptyList = 
            toList nonEmptyList
            |> List.sortBy keySelect
            |> fromListUnsafe

        let sortByDescending keySelect nonEmptyList = 
            toList nonEmptyList
            |> List.sortByDescending keySelect
            |> fromListUnsafe

        let groupBy keySelect nonEmptyList =
            toList nonEmptyList
            |> List.groupBy keySelect
            |> fromListUnsafe
            |> map (tupleMapSecond fromListUnsafe)

        let choose predicate nonEmptyList =
            toList nonEmptyList
            |> List.choose predicate

        let join innerKeySelector outerKeySelector inner outer =
            seq {
                for outer in (toList outer |> List.toSeq) do
                for inner in (List.toSeq inner) do
                    if outerKeySelector outer = innerKeySelector inner then
                        yield (inner, outer)
            }            

        let length nonEmptyList =
            List.length nonEmptyList.Tail
            |> (+) 1

        let append second first = {
            NonEmptyList.Head = first.Head
            NonEmptyList.Tail = first.Tail @ (toList second) }

        let head nonEmptyList = nonEmptyList.Head
        let tail nonEmptyList = nonEmptyList.Tail

    let tupleNonEmptyListSplit (items: NonEmptyList<('a * 'b)>): (NonEmptyList<'a> * NonEmptyList<'b>) = 
        let firsts = NonEmptyList.map tupleFirst items
        let seconds = NonEmptyList.map tupleSecond items
        (firsts, seconds)