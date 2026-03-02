-- Database Initialization Script

-- Table: Ambulatorios
CREATE TABLE IF NOT EXISTS Ambulatorios (
    nroa INT PRIMARY KEY,
    andar INT NOT NULL,
    capacidade INT NOT NULL
);

-- Table: Pacientes
CREATE TABLE IF NOT EXISTS Pacientes (
    codp INT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    idade INT NOT NULL,
    cidade VARCHAR(100) NOT NULL,
    RG VARCHAR(20) NOT NULL,
    problema VARCHAR(100) NOT NULL
);

-- Table: Medicos
CREATE TABLE IF NOT EXISTS Medicos (
    codm INT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    idade INT NOT NULL,
    especialidade VARCHAR(100) NOT NULL,
    RG VARCHAR(20) NOT NULL,
    cidade VARCHAR(100) NOT NULL,
    nroa INT,
    FOREIGN KEY (nroa) REFERENCES Ambulatorios(nroa)
);

-- Table: Consultas
CREATE TABLE IF NOT EXISTS Consultas (
    codm INT,
    codp INT,
    data DATE NOT NULL,
    hora TIME NOT NULL,
    PRIMARY KEY (codm, codp, data, hora),
    FOREIGN KEY (codm) REFERENCES Medicos(codm),
    FOREIGN KEY (codp) REFERENCES Pacientes(codp)
);

-- Population: Ambulatorios
INSERT IGNORE INTO Ambulatorios (nroa, andar, capacidade) VALUES
(1, 1, 30),
(2, 1, 50),
(3, 2, 40),
(4, 2, 25),
(5, 2, 50);

-- Population: Medicos
INSERT IGNORE INTO Medicos (codm, nome, idade, especialidade, RG, cidade, nroa) VALUES
(1, 'João', 40, 'ortopedia', '1000010000', 'Fpolis', 1),
(2, 'Maria', 42, 'traumatologia', '1000011000', 'Blumenau', 2),
(3, 'Pedro', 51, 'pediatria', '1100010000', 'Fpolis', 2),
(4, 'Carlos', 28, 'ortopedia', '1100011000', 'Joinville', NULL),
(5, 'Márcia', 33, 'neurologia', '1100011100', 'Biguaçu', 3);

-- Population: Pacientes
INSERT IGNORE INTO Pacientes (codp, nome, idade, cidade, RG, problema) VALUES
(1, 'Ana', 20, 'Fpolis', '2000020000', 'gripe'),
(2, 'Paulo', 24, 'Palhoça', '2000022000', 'fratura'),
(3, 'Lúcia', 30, 'Fpolis', '2200020000', 'tendinite'),
(4, 'Carlos', 28, 'Joinville', '1100011000', 'sarampo'),
(5, 'Jorge', 9, 'Joinville', '2222022200', 'câncer'),
(6, 'Marcos', 45, 'Blumenau', '2222200000', 'tendinite');

-- Population: Consultas
-- Note: Dates are converted from MM/DD/YY to YYYY-MM-DD
INSERT IGNORE INTO Consultas (codm, codp, data, hora) VALUES
(1, 1, '2002-11-12', '14:00:00'),
(1, 4, '2002-11-13', '10:00:00'),
(2, 1, '2002-11-13', '09:00:00'),
(2, 2, '2002-11-13', '11:00:00'),
(2, 3, '2002-11-14', '14:00:00'),
(2, 4, '2002-11-14', '17:00:00'),
(3, 1, '2002-11-17', '18:00:00'),
(3, 3, '2002-11-12', '10:00:00'),
(3, 4, '2002-11-15', '13:00:00'),
(4, 4, '2002-11-16', '13:00:00');

-- Table: Funcionarios
CREATE TABLE IF NOT EXISTS Funcionarios (
    codf INT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    idade INT NOT NULL,
    RG VARCHAR(20) NOT NULL,
    salario DECIMAL(10, 2) NOT NULL,
    depto VARCHAR(100) NOT NULL,
    tempoServico INT NOT NULL
);

-- Population: Funcionarios
INSERT IGNORE INTO Funcionarios (codf, nome, idade, RG, salario, depto, tempoServico) VALUES
(1, 'Maria', 42, '1000011000', 700.00, 'pessoal', 2),
(2, 'Lúcia', 30, '2200020000', 1500.00, 'pessoal', 1),
(3, 'Marcelo', 27, '3000030000', 1000.00, 'contábil', 4),
(4, 'Ricardo', 33, '3300030000', 1200.00, 'contábil', 2),
(5, 'José', 18, '3330030000', 180.00, 'limpeza', 3);
