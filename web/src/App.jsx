import { useState } from 'react'
import './App.css'
import GenericCrud from './components/GenericCrud'

const UserTransactionTest = () => {
  const [formData, setFormData] = useState({
    codp: '', nome: '', idade: '', cidade: '', rg: '', problema: ''
  });

  const handleSubmit = async (acao) => {
    try {
      const response = await fetch(`http://localhost:8080/api/transacao-manual/${acao}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          codp: Number(formData.codp),
          nome: formData.nome,
          idade: Number(formData.idade),
          cidade: formData.cidade,
          RG: formData.rg,
          problema: formData.problema
        })
      });
      const data = await response.json();
      alert(data.message || data.detail || 'Erro na operação');
    } catch(e) {
      alert("Erro ao conectar com a API: " + e.message);
    }
  };

  return (
    <div className="card test-transaction-card">
      <h2 style={{color: '#d9534f', marginTop: 0}}>⚠️ Banco de Dados: Transação Manual</h2>
      <p style={{marginBottom: "20px", fontSize: "1.1rem"}}>
        Preencha os dados de um novo Paciente e escolha se deseja <strong>confirmar</strong> a inserção de vez no banco de dados (Commit) ou <strong>desfazer</strong> as alterações completamente (Rollback).
      </p>
      
      <div className="crud-form" style={{maxWidth: '500px', margin: '0 auto', textAlign: 'left'}}>
        <div className="form-group">
          <label>Cód. Paciente:</label>
          <input type="number" onChange={e => setFormData({...formData, codp: e.target.value})} />
        </div>
        <div className="form-group">
          <label>Nome:</label>
          <input type="text" onChange={e => setFormData({...formData, nome: e.target.value})} />
        </div>
        <div className="form-group">
          <label>Idade:</label>
          <input type="number" onChange={e => setFormData({...formData, idade: e.target.value})} />
        </div>
        <div className="form-group">
          <label>Cidade:</label>
          <input type="text" onChange={e => setFormData({...formData, cidade: e.target.value})} />
        </div>
        <div className="form-group">
          <label>RG:</label>
          <input type="text" onChange={e => setFormData({...formData, rg: e.target.value})} />
        </div>
        <div className="form-group">
          <label>Problema:</label>
          <input type="text" onChange={e => setFormData({...formData, problema: e.target.value})} />
        </div>
      </div>
      
      <div style={{marginTop: "30px", display: "flex", justifyContent: "center", gap: "15px"}}>
        <button 
          style={{backgroundColor: "#28a745", padding: "12px 24px", fontSize: "1.1em"}}
          onClick={() => handleSubmit("COMMIT")}>
          ✅ Salvar e Confirmar (COMMIT)
        </button>
        <button 
          style={{backgroundColor: "#dc3545", padding: "12px 24px", fontSize: "1.1em"}}
          onClick={() => handleSubmit("ROLLBACK")}>
          ↩️ Desfazer Alteração (ROLLBACK)
        </button>
      </div>
    </div>
  )
}

function App() {
  const [activeTab, setActiveTab] = useState('ambulatorios')
  const [reportSql, setReportSql] = useState('')
  const [showReport, setShowReport] = useState(false)

  const handleGenerateReport = async () => {
    try {
      // Usar variável de ambiente ou hardcoded pra fins escolares
      const baseUrl = 'http://localhost:8080';
      const response = await fetch(`${baseUrl}/api/relatorio/sql`);
      if (response.ok) {
        const text = await response.text();
        setReportSql(text);
        setShowReport(true);
      } else {
        alert("Erro ao buscar relatório.");
      }
    } catch (e) {
      alert("Erro ao gerar relatório SQL.");
      console.error(e);
    }
  }

  const tables = {
    ambulatorios: {
      title: 'Ambulatórios',
      endpoint: 'ambulatorios',
      primaryKey: 'nroa',
      fields: [
        { name: 'nroa', label: 'Nº Ambulatório', type: 'number', required: true, isPK: true },
        { name: 'andar', label: 'Andar', type: 'number', required: true },
        { name: 'capacidade', label: 'Capacidade', type: 'number', required: true },
      ]
    },
    medicos: {
      title: 'Médicos',
      endpoint: 'medicos',
      primaryKey: 'codm',
      fields: [
        { name: 'codm', label: 'Cód. Médico', type: 'number', required: true, isPK: true },
        { name: 'nome', label: 'Nome', required: true },
        { name: 'idade', label: 'Idade', type: 'number', required: true },
        { name: 'especialidade', label: 'Especialidade', required: true },
        { name: 'rg', label: 'RG', required: true },
        { name: 'cidade', label: 'Cidade', required: true },
        { name: 'nroa', label: 'Nº Ambulatório', type: 'number' },
      ]
    },
    pacientes: {
      title: 'Pacientes',
      endpoint: 'pacientes',
      primaryKey: 'codp',
      fields: [
        { name: 'codp', label: 'Cód. Paciente', type: 'number', required: true, isPK: true },
        { name: 'nome', label: 'Nome', required: true },
        { name: 'idade', label: 'Idade', type: 'number', required: true },
        { name: 'cidade', label: 'Cidade', required: true },
        { name: 'rg', label: 'RG', required: true },
        { name: 'problema', label: 'Problema', required: true },
      ]
    },
    consultas: {
      title: 'Consultas',
      endpoint: 'consultas',
      primaryKey: ['codm', 'codp', 'data', 'hora'],
      fields: [
        { name: 'codm', label: 'Cód. Médico', type: 'number', required: true, isPK: true },
        { name: 'codp', label: 'Cód. Paciente', type: 'number', required: true, isPK: true },
        { name: 'data', label: 'Data', type: 'date', required: true, isPK: true },
        { name: 'hora', label: 'Hora', type: 'time', required: true, isPK: true },
      ]
    },
    funcionarios: {
      title: 'Funcionários',
      endpoint: 'funcionarios',
      primaryKey: 'codf',
      fields: [
        { name: 'codf', label: 'Cód. Func.', type: 'number', required: true, isPK: true },
        { name: 'nome', label: 'Nome', required: true },
        { name: 'idade', label: 'Idade', type: 'number', required: true },
        { name: 'rg', label: 'RG', required: true },
        { name: 'salario', label: 'Salário', type: 'number', required: true },
        { name: 'depto', label: 'Departamento', required: true },
        { name: 'tempoServico', label: 'Tempo Serviço', type: 'number', required: true },
      ]
    }
  }

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>Sistema Hospitalar UFMT</h1>
        <button className="btn-report" onClick={handleGenerateReport}>
          💾 Gerar Relatório SQL
        </button>
      </header>
      <nav className="tab-menu">
        {Object.keys(tables).map(key => (
          <button 
            key={key} 
            className={activeTab === key ? 'active' : ''} 
            onClick={() => setActiveTab(key)}
          >
            {tables[key].title}
          </button>
        ))}
        <button 
          className={activeTab === 'transacao_manual' ? 'active' : ''} 
          onClick={() => setActiveTab('transacao_manual')}
          style={{backgroundColor: '#ffeeba', color: '#856404', borderColor: '#ffeeba', fontWeight: 'bold'}}
        >
          ⚙️ Testar DB Transaction
        </button>
      </nav>
      <main>
        {activeTab === 'transacao_manual' ? (
          <UserTransactionTest />
        ) : (
          <GenericCrud {...tables[activeTab]} />
        )}
      </main>

      {showReport && (
        <div className="modal-overlay" onClick={() => setShowReport(false)}>
          <div className="modal-content report-modal" onClick={e => e.stopPropagation()}>
            <h3>Relatório do Banco de Dados (SQL)</h3>
            <p>Abaixo estão os comandos INSERT baseados no estado atual do banco:</p>
            <pre>{reportSql}</pre>
            <div className="modal-actions">
              <button className="btn-cancel" onClick={() => setShowReport(false)}>Fechar Relatório</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default App
