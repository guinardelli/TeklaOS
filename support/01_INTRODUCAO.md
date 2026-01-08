# INSTRUCAO.md - Guia de Desenvolvimento Tekla Structures 2023 (Open API)

**Projeto:** Sistema de Utilitários, Relatórios e Manutenção para Tekla Structures 2023.
**Tech Stack:** C# (.NET Framework 4.8), WinForms, Tekla Open API.
**Integração:** Plugin via Applications & Components Catalog.

---

## 1. Introdução à Programação no Tekla Structures

A automação no Tekla 2023 via Open API opera sob uma arquitetura cliente-servidor, onde sua aplicação (.DLL ou .EXE) se comunica com o processo principal do `TeklaStructures.exe`.

### Assemblies Principais (References)
* **Tekla.Structures.dll**: Núcleo e tipos básicos.
* **Tekla.Structures.Model.dll**: Manipulação do banco de dados 3D (Partes, Assembly, UDAs).
* **Tekla.Structures.Dialog.dll**: Base para WinForms integrados (`PluginFormBase`).
* **Tekla.Structures.Datatype.dll**: Tratamento de unidades e tipos de dados do Tekla.
* **Tekla.Structures.Catalogs.dll**: Acesso a catálogos de perfil e material.

### Ciclo de Vida do Plugin
1.  **Inicialização**: O Tekla carrega a DLL da pasta `environments\common\extensions` ou via TSEP.
2.  **Execução**: O usuário clica no ícone em "Applications & Components".
3.  **Lógica**: A classe herdada de `PluginBase` executa o método `Run()`.
4.  **UI**: A classe herdada de `PluginFormBase` gerencia a interface WinForms.

---