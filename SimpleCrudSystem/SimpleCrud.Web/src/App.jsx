import { useState } from 'react'
import './App.css'
import GenericCrud from './components/GenericCrud'

function App() {
  const [activeTab, setActiveTab] = useState('ambulatorios')

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
        { name: 'RG', label: 'RG', required: true },
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
        { name: 'RG', label: 'RG', required: true },
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
    }
  }

  return (
    <div className="app-container">
      <h1>Sistema Hospitalar UFMT</h1>
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
      </nav>
      <main>
        <GenericCrud {...tables[activeTab]} />
      </main>
    </div>
  )
}

export default App
