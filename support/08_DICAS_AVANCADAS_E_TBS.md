8. Dicas Avançadas e Troubleshooting
Macro vs Plugin: Para utilitários complexos com UI, prefira compilá-los como .exe externo que se conecta ao Tekla (Model.Connect()) ou Plugins DLL. Scripts C# (.cs) são limitados para UI complexa.

Model Sharing: Ao modificar objetos em um ambiente Model Sharing, certifique-se de não reter IDs ou GUIDs por muito tempo, pois eles podem mudar ou ser bloqueados.

Logs: Use Operation.DisplayMessageTag("Msg") para logs rápidos na barra inferior do Tekla. Para logs complexos, escreva em .txt na pasta do modelo (model.GetInfo().ModelPath).

Distribuição: Para distribuir via Tekla Warehouse, empacote como .tsep (Tekla Structures Extension Package), definindo o manifesto XML para copiar a DLL para <TeklaVersion>\nt\bin\plugins.

Aprendizados recentes (AssemblyComparator / Tekla 2023)
- O compilador de macros do Tekla 2023 e antigo. Evite pattern matching `is Type var` e interpolacao `$"..."`. Use cast + null check e `string.Format`.
- `Assembly.GetMembers()` nao existe no Tekla 2023. Use `GetMainPart()` + `GetSecondaries()`.
- `GetMainPart()` retorna `ModelObject`. Use `Part mainPart = ass.GetMainPart() as Part;`.
- `GetSecondaries()` retorna `ArrayList`. Percorra com `foreach (ModelObject obj in list)` e faca cast para `Part`.
- Se aparecer CS0266/CS0029, revise tipos de retorno e use cast explicito.
