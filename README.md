# ApiRef
ApiRef é uma ferramenta simples que realiza a leitura de arquivos .dll (C#) e .xml e gera arquivos .md de documentação/API Reference.

# CLI
```
/caminho/do/apiref biblioteca.dll [argumentos...]
```

## Argumentos Opcionais
- `-o/--output`: Define o destino do resultado. `./api` por padrão.
- `--all`: Desabilita o filtro por campos `public` e `protected`.
