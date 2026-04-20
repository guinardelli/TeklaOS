internal static class DrawingModelLinker
{
    // Selecionar no modelo a partir da marca digitada do desenho
    public static void SelectModelPartFromDrawingMark(string mark)
    {
        if (string.IsNullOrWhiteSpace(mark))
        {
            MessageBox.Show("Por favor, digite o nome/marca do desenho.", "Caixa Vazia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        mark = mark.Trim();
        Tekla.Structures.Drawing.DrawingHandler drawingHandler = new Tekla.Structures.Drawing.DrawingHandler();
        Tekla.Structures.Drawing.DrawingEnumerator drawings = drawingHandler.GetDrawings();
        
        bool found = false;
        Model model = new Model();

        while (drawings.MoveNext())
        {
            Tekla.Structures.Drawing.Drawing drawing = drawings.Current;
            // Verifica tanto a marca quanto o titulo
            if (drawing.Mark.Equals(mark, StringComparison.InvariantCultureIgnoreCase) || 
                drawing.Name.Equals(mark, StringComparison.InvariantCultureIgnoreCase))
            {
                Identifier modelIdentifier = null;

                Tekla.Structures.Drawing.AssemblyDrawing ad = drawing as Tekla.Structures.Drawing.AssemblyDrawing;
                Tekla.Structures.Drawing.SinglePartDrawing sp = drawing as Tekla.Structures.Drawing.SinglePartDrawing;
                Tekla.Structures.Drawing.CastUnitDrawing cu = drawing as Tekla.Structures.Drawing.CastUnitDrawing;

                if (ad != null)
                {
                    modelIdentifier = ad.AssemblyIdentifier;
                }
                else if (sp != null)
                {
                    modelIdentifier = sp.PartIdentifier;
                }
                else if (cu != null)
                {
                    modelIdentifier = cu.CastUnitIdentifier;
                }

                if (modelIdentifier != null)
                {
                    ModelObject modelObj = model.SelectModelObject(modelIdentifier);
                    if (modelObj != null)
                    {
                        var sel = new ModelUI.ModelObjectSelector();
                        sel.Select(new ArrayList() { modelObj });

                        // Destacar visualmente com prompt
                        Tekla.Structures.Model.Operations.Operation.DisplayPrompt(string.Format("Desenho [{0}] -> Peça do modelo base selecionada!", drawing.Mark));
                        
                        // Bônus: Focar na peça via execução de macro (Zoom to Selected)
                        Tekla.Structures.Model.Operations.Operation.RunMacro("ZoomToSelected");
                        
                        found = true;
                        break;
                    }
                }
            }
        }

        if (!found)
        {
            System.Windows.Forms.MessageBox.Show(string.Format("Desenho '{0}' não encontrado na lista de desenhos, ou peça base não existe mais no modelo.\n\nVerifique se o nome confere exatamente com o 'Mark' ou 'Name' do Document Manager.", mark), 
                "Não encontrado", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }
    }

    // Seleção reversa: Abrir desenho a associado a partir de objeto selecionado no modelo
    public static void OpenDrawingFromSelectedModelPart()
    {
        var sel = new ModelUI.ModelObjectSelector();
        var selectedEnum = sel.GetSelectedObjects();
        
        if (selectedEnum.GetSize() == 0)
        {
             MessageBox.Show("Você precisa selecionar um objeto no modelo 3D primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
             return;
        }
        
        selectedEnum.MoveNext();
        ModelObject selectedObj = selectedEnum.Current;
        Model model = new Model();
        
        string targetMark = "";
        
        Assembly assem = selectedObj as Assembly;
        Part p = selectedObj as Part;

        if (assem != null)
        {
            targetMark = ModelHelper.GetReportProperty(assem, "ASSEMBLY_POS");
        }
        else if (p != null)
        {
            Assembly parentAssembly = p.GetAssembly();
            if (parentAssembly != null)
            {
                targetMark = ModelHelper.GetReportProperty(parentAssembly, "ASSEMBLY_POS");
            }
            else
            {
                targetMark = ModelHelper.GetReportProperty(p, "PART_POS");
            }
        }

        if (string.IsNullOrEmpty(targetMark) || targetMark == "-") 
        {
            MessageBox.Show("Não foi possível determinar a marca (posição) do objeto selecionado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Tekla.Structures.Drawing.DrawingHandler drawingHandler = new Tekla.Structures.Drawing.DrawingHandler();
        Tekla.Structures.Drawing.DrawingEnumerator drawings = drawingHandler.GetDrawings();
        bool found = false;
        
        while(drawings.MoveNext())
        {
            Tekla.Structures.Drawing.Drawing drawing = drawings.Current;
            // No Tekla, desenhos de peças identicas recebem o mesmo Drawing.Mark da posição
            if (drawing.Mark.Equals(targetMark, StringComparison.InvariantCultureIgnoreCase))
            {
                // Abre o desenho
                drawingHandler.SetActiveDrawing(drawing, true);
                found = true;
                break;
            }
        }

        if(!found)
        {
             System.Windows.Forms.MessageBox.Show(string.Format("Nenhum desenho encontrado para a posição '{0}' no Document Manager.\n\nProvavelmente o desenho ainda não foi criado.", targetMark), "Não encontrado", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }
    }
}
