# ApiRef
ApiRef � uma ferramenta simples que realiza a leitura de arquivos .dll (C#) e .xml e gera arquivos .md de documenta��o/API Reference.

# CLI
```
/caminho/do/apiref biblioteca.dll [argumentos...]
```

## Argumentos Opcionais
- `-o/--output`: Define o destino do resultado. `./api` por padr�o.
- `--all`: Desabilita o filtro por campos `public` e `protected`.
