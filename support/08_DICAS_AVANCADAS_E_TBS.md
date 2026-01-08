8. Dicas Avançadas e Troubleshooting
Macro vs Plugin: Para utilitários complexos com UI, prefira compilá-los como .exe externo que se conecta ao Tekla (Model.Connect()) ou Plugins DLL. Scripts C# (.cs) são limitados para UI complexa.

Model Sharing: Ao modificar objetos em um ambiente Model Sharing, certifique-se de não reter IDs ou GUIDs por muito tempo, pois eles podem mudar ou ser bloqueados.

Logs: Use Operation.DisplayMessageTag("Msg") para logs rápidos na barra inferior do Tekla. Para logs complexos, escreva em .txt na pasta do modelo (model.GetInfo().ModelPath).

Distribuição: Para distribuir via Tekla Warehouse, empacote como .tsep (Tekla Structures Extension Package), definindo o manifesto XML para copiar a DLL para <TeklaVersion>\nt\bin\plugins.