4. Boas Práticas de Codificação
Gerenciamento de Transações
Sempre confirme as alterações para que elas apareçam no modelo.

var model = new Tekla.Structures.Model.Model();
// ... modificações no modelo ...
model.CommitChanges(); // OBRIGATÓRIO para persistir dados

Tratamento de Coordinate Systems (Work Plane)
Para evitar erros de geometria, alterne para o plano de trabalho correto e restaure depois.

var model = new Model();
var workPlaneHandler = model.GetWorkPlaneHandler();
var originalPlane = workPlaneHandler.GetCurrentTransformationPlane();

try 
{
    // Mudar para plano global ou local conforme necessidade
    workPlaneHandler.SetCurrentTransformationPlane(new TransformationPlane());
    
    // ... Operações geométricas ...
}
catch (Exception ex)
{
    Tekla.Structures.Model.Operations.Operation.DisplayMessageTag("Erro: " + ex.Message);
}
finally
{
    // Restaurar plano original sempre
    workPlaneHandler.SetCurrentTransformationPlane(originalPlane);
}

Performance
Use GetAllObjects() com cuidado. Prefira GetObjectsByBoundingBox() ou Seletores.

Em loops grandes, evite chamadas excessivas de CommitChanges(). Faça em lote.