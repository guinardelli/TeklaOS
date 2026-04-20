internal static class MaterialSummary
    {
        public static string BuildMaterialReport(bool onlySelected)
        {
            Model model = ModelHelper.GetConnectedModel();
            if (model == null) return null;

            ModelObjectEnumerator enumerator;
            if (onlySelected)
            {
                var uiSelector = new ModelUI.ModelObjectSelector();
                enumerator = uiSelector.GetSelectedObjects();
                if (enumerator == null || enumerator.GetSize() == 0)
                {
                    MessageBox.Show("Nenhuma peca selecionada.", "Resumo de Materiais", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }
            }
            else
            {
                enumerator = model.GetModelObjectSelector().GetAllObjectsWithType(ModelObject.ModelObjectEnum.UNKNOWN);
                if (enumerator == null) return null;
            }

            var matProfileQuantities = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
            var matProfileWeights = new Dictionary<string, Dictionary<string, double>>(StringComparer.OrdinalIgnoreCase);

            var previousCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            int totalParts = 0;

            try
            {
                int processed = 0;
                while (enumerator.MoveNext())
                {
                    processed++;
                    if (processed % 1000 == 0) Application.DoEvents();

                    ModelObject obj = enumerator.Current as ModelObject;
                    if (obj == null) continue;

                    // Se a selecao retornou Assembly, extrair as parts internas
                    Assembly assembly = obj as Assembly;
                    if (assembly != null)
                    {
                        ArrayList secondaries = assembly.GetSecondaries();
                        ProcessPart(assembly.GetMainPart() as Part, matProfileQuantities, matProfileWeights, ref totalParts);
                        if (secondaries != null)
                        {
                            foreach (ModelObject sec in secondaries)
                            {
                                ProcessPart(sec as Part, matProfileQuantities, matProfileWeights, ref totalParts);
                            }
                        }
                    }
                    else
                    {
                        Part part = obj as Part;
                        if (part != null)
                        {
                            ProcessPart(part, matProfileQuantities, matProfileWeights, ref totalParts);
                        }
                    }
                }
            }
            finally
            {
                Cursor.Current = previousCursor;
            }

            if (totalParts == 0)
            {
                MessageBox.Show("Nenhuma peca encontrada.", "Resumo de Materiais", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return FormatReport(matProfileQuantities, matProfileWeights, totalParts, onlySelected);
        }

        private static void ProcessPart(Part part, 
            Dictionary<string, Dictionary<string, int>> matProfileQuantities, 
            Dictionary<string, Dictionary<string, double>> matProfileWeights,
            ref int totalParts)
        {
            if (part == null) return;

            string material = part.Material.MaterialString;
            string profile = part.Profile.ProfileString;
            if (string.IsNullOrWhiteSpace(material)) material = "INDEFINIDO";
            if (string.IsNullOrWhiteSpace(profile)) profile = "INDEFINIDO";

            double weight = 0.0;
            part.GetReportProperty("WEIGHT_NET", ref weight);

            if (!matProfileQuantities.ContainsKey(material))
            {
                matProfileQuantities[material] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                matProfileWeights[material] = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            }

            var pDict = matProfileQuantities[material];
            if (!pDict.ContainsKey(profile)) pDict[profile] = 0;
            pDict[profile]++;

            var wDict = matProfileWeights[material];
            if (!wDict.ContainsKey(profile)) wDict[profile] = 0;
            wDict[profile] += weight;

            totalParts++;
        }

        private static string FormatReport(
            Dictionary<string, Dictionary<string, int>> matProfileQuantities,
            Dictionary<string, Dictionary<string, double>> matProfileWeights,
            int totalParts,
            bool onlySelected)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== RESUMO DE MATERIAIS ===");
            sb.AppendLine(string.Format("Data: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm")));
            sb.AppendLine(string.Format("Modalidade: {0}", onlySelected ? "Pecas selecionadas" : "Modelo inteiro"));
            sb.AppendLine(string.Format("Total de pecas avaliadas: {0}", totalParts));
            sb.AppendLine();

            var sortedMaterials = new List<string>(matProfileQuantities.Keys);
            sortedMaterials.Sort(StringComparer.OrdinalIgnoreCase);

            double totalWeightAll = 0.0;

            foreach (string material in sortedMaterials)
            {
                sb.AppendLine(string.Format("Material: {0}", material));
                
                var pDict = matProfileQuantities[material];
                var wDict = matProfileWeights[material];

                var sortedProfiles = new List<string>(pDict.Keys);
                sortedProfiles.Sort(StringComparer.OrdinalIgnoreCase);

                int subtotalQuant = 0;
                double subtotalWeight = 0.0;

                foreach (string profile in sortedProfiles)
                {
                    int q = pDict[profile];
                    double w = wDict[profile];
                    subtotalQuant += q;
                    subtotalWeight += w;

                    sb.AppendLine(string.Format("  {0,-15} | {1,5} pçs | {2,10:N1} kg", profile, q, w));
                }

                sb.AppendLine(string.Format("  Subtotal:       | {0,5} pçs | {1,10:N1} kg", subtotalQuant, subtotalWeight));
                sb.AppendLine();

                totalWeightAll += subtotalWeight;
            }

            sb.AppendLine("=== TOTAIS GERAIS ===");
            sb.AppendLine(string.Format("Total de pecas: {0}", totalParts));
            sb.AppendLine(string.Format("Peso Total    : {0:N1} kg", totalWeightAll));
            
            return sb.ToString();
        }
    }
