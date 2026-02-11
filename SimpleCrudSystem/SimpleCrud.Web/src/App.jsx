import { useState } from 'react'
import './App.css'
import ItemList from './components/ItemList'
import ItemForm from './components/ItemForm'

function App() {
  const [itemToEdit, setItemToEdit] = useState(null)
  const [refresh, setRefresh] = useState(false)

  const handleEdit = (item) => {
    setItemToEdit(item)
  }

  const handleSave = () => {
    setItemToEdit(null)
    setRefresh(!refresh)
  }

  return (
    <>
      <h1>Simple CRUD System</h1>
      <ItemForm itemToEdit={itemToEdit} onSave={handleSave} />
      <ItemList onEdit={handleEdit} refresh={refresh} />
    </>
  )
}

export default App
