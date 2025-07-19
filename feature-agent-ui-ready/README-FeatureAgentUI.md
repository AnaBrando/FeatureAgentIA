# 🧠 Feature Agent UI

Uma interface React para gerar histórias de backlog técnicas usando IA.  
A aplicação envia um prompt estruturado para uma API (como OpenAI) e retorna um arquivo `.md` com:

- Contexto de negócio
- Plano de implementação
- Critérios de qualidade com BDD

---

## 🚀 Como rodar o projeto

### 📦 1. Clone ou extraia o projeto
Se você baixou o `.zip`, extraia-o para alguma pasta.

### 📁 2. Acesse a pasta
```bash
cd feature-agent-ui-ready
```

### 📥 3. Instale as dependências
```bash
npm install
```

### 🖥️ 4. Rode o projeto localmente
```bash
npm run dev
```

Abra no navegador:  
👉 [`http://localhost:5173`](http://localhost:5173)

---

## 🔌 Backend necessário

Este frontend espera um backend escutando em:

```
http://localhost:5000/generate-feature
```

> Certifique-se de que seu backend .NET esteja rodando corretamente antes de testar.

---

## 🛠 Tecnologias usadas

- React 18
- Vite
- Tailwind CSS
- shadcn/ui
- OpenAI API (via seu backend)

---
