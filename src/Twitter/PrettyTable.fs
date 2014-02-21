module PrettyTable

open System.Drawing
open System.Windows.Forms

let buildForm title =
    let form = new Form(Visible = true, Text = title,
                        TopMost = true, Size = Size(600,600))

    let data = new DataGridView(Dock = DockStyle.Fill, Text = "F#",
                                Font = new Font("Lucida Console",12.0f),
                                ForeColor = Color.DarkBlue)
 
    form.Controls.Add(data)
    data

let show title dataSource =
    let data = buildForm title
    data.DataSource <- (dataSource |> Array.ofSeq)

type CountRow = { Name:string ; Count:int }
let showCounts title dataSource =    
    let getColumnMaxLength f dataSource =
        dataSource 
            |> Array.map f
            |> Array.map (fun d -> d.ToString() |> String.length)
            |> Array.max
            |> (*) 15

    let firstColumnMaxLength = dataSource |> getColumnMaxLength (fun d -> d.Name)    
    let secondColumnMaxLength = dataSource |> getColumnMaxLength (fun d -> d.Count)

    let data = buildForm title
    
    data.DataSource <- dataSource
    data.Columns.[0].Width <- firstColumnMaxLength
    data.Columns.[1].Width <- secondColumnMaxLength
      
type StringRow = { Text:string }       
let showListOfStrings title strings =
    let dataSource = strings |> Seq.map (fun s -> { Text = s })
    
    let data = buildForm title

    data.DataSource <- (dataSource |> Array.ofSeq)