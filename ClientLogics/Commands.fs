module Commands

open System
open System.Windows.Input

///Silverlight Command pattern
type public ChatCommand(f) =
    let cec = new DelegateEvent<EventHandler>()
    interface ICommand with 
        member x.CanExecute p = 
            (p <> null) && (p.ToString() <> "")
        member x.Execute p = f(p) 
        [<CLIEvent>]
        member x.CanExecuteChanged = cec.Publish