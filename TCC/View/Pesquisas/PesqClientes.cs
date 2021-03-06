﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TCC.Model;
using TCC.Model.Classes;
using TCC.Model.DAO;
using TCC.View.Add;

namespace TCC.View.Pesquisas
{
    public partial class PesqClientes : Form
    {
        private ModelDB modelDB;
        private ClientesDAO clientesDAO { get; set; }
        private CidadesDAO cidadesDAO { get; set; }
        private EstadosDAO estadosDAO { get; set; }
        private ObservacoesDAO observacoesDAO { get; set; }
        private Observacoes observacoes;
        private EnvioDeEmail envioDeEmail;
        private AddObservacao addObservacao;
        private DialogResult resposta;
        private IEnumerable<Clientes> listaClientes;
        private DataGridViewImageColumn img, img2;
        private bool check, existe;
        private int idCliente;

        public PesqClientes()
        {
            InitializeComponent();
            modelDB = new ModelDB();
            clientesDAO = new ClientesDAO();
            cidadesDAO = new CidadesDAO();
            estadosDAO = new EstadosDAO();
            observacoesDAO = new ObservacoesDAO();
            check = false;
        }

        private void PesqClientes_Load(object sender, EventArgs e)
        {
            #region Inicialização do dataGridView (criação das colunas)
            // Adiciona as colunas a serem exibidas (conteúdo, título da coluna)
            dataGridView.Columns.Add("Id", "Código");
            dataGridView.Columns.Add("Nome", "Nome");
            dataGridView.Columns.Add("Email", "E-mail");
            dataGridView.Columns.Add("Estado.Sigla", "UF");
            dataGridView.Columns.Add("Cidade.Nome", "Cidade");
            dataGridView.Columns.Add("Endereco", "Endereço");
            dataGridView.Columns.Add("Telefone", "Telefone");

            // Criação da coluna de imagens
            img = new DataGridViewImageColumn();
            img.Image = Variaveis.getEmail();
            dataGridView.Columns.Add(img);
            img.HeaderText = "";
            img.Name = "img";
            img.ImageLayout = DataGridViewImageCellLayout.Zoom;

            img2 = new DataGridViewImageColumn();
            img2.Image = Variaveis.getObs();
            dataGridView.Columns.Add(img2);
            img2.HeaderText = "";
            img2.Name = "img2";
            img2.ImageLayout = DataGridViewImageCellLayout.Zoom;

            // Largura das colunas (o default é 100)
            dataGridView.Columns["Id"].Width = 50;
            dataGridView.Columns["Nome"].Width = 200;
            dataGridView.Columns["Email"].Width = 200;
            dataGridView.Columns["Estado.Sigla"].Width = 35;
            dataGridView.Columns["img"].Width = 35;
            dataGridView.Columns["img2"].Width = 35;
            dataGridView.Columns["Endereco"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            dataGridView.RowsDefaultCellStyle.BackColor = Color.AliceBlue;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;
            #endregion

            #region Carregar cidades no comboBox
            try
            {
                comboFCidade.DataSource = cidadesDAO.select().ToList();
                comboFCidade.ValueMember = "Nome";
                comboFCidade.DisplayMember = "Nome";

                // Habilitar o autoComplete
                comboFCidade.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboFCidade.AutoCompleteSource = AutoCompleteSource.ListItems;
                comboFCidade.Text = "";
            }
            catch
            {
                
            }
            #endregion
        }

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            #region Set the Cell's ToolTipText
            if (e.ColumnIndex == dataGridView.Columns["img"].Index)
            {
                var cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.ToolTipText = "Enviar E-mail";
            }
            else if (e.ColumnIndex == dataGridView.Columns["img2"].Index)
            {
                var cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.ToolTipText = "Adicionar Observação";
            }
            #endregion
        }

        private void PesqClientes_Activated(object sender, EventArgs e)
        {
            carregarClientes();
        }

        private void carregarClientes()
        {
            dataGridView.Rows.Clear();

            // Check->false: caso tenha feito uma pesquisa, não reseta as linhas do dataGrid até
            // que o usuário aperte no botão "Limpar" ou feche o form
            if (check == false)
            {
                #region Carregar clientes no dataGridView
                try
                {
                    // preenche as colunas
                    foreach (Clientes c in clientesDAO.select())
                    {
                        Cidades cid = cidadesDAO.selectCidade(c.Cidade.Id);
                        dataGridView.Rows.Add(c.Id, c.Nome, c.Email, cid.Estado.Sigla, c.Cidade.Nome, c.Endereco, c.Telefone);
                    }
                }
                catch
                {
                    
                }
                #endregion
            }
            else
            {
                #region Carregar clientes que se encaixam na pesquisa
                try
                {
                    if (!textFNome.Text.Trim().Equals("") && !comboFCidade.Text.Trim().Equals(""))
                    {
                        listaClientes = clientesDAO.select().Where(x => x.Nome.ToUpper().Contains(textFNome.Text.Trim().ToUpper()) && x.Cidade.Nome.ToUpper().Contains(comboFCidade.Text.Trim().ToUpper()));
                    }
                    else if (!textFNome.Text.Trim().Equals(""))
                    {
                        listaClientes = clientesDAO.select().Where(x => x.Nome.ToUpper().Contains(textFNome.Text.Trim().ToUpper()));
                    }
                    else if (!comboFCidade.Text.Trim().Equals(""))
                    {
                        listaClientes = clientesDAO.select().Where(x => x.Cidade.Nome.ToUpper().Contains(comboFCidade.Text.Trim().ToUpper()));
                    }

                    // preenche as colunas
                    foreach (Clientes c in listaClientes)
                    {
                        Cidades cid = cidadesDAO.selectCidade(c.Cidade.Id);
                        dataGridView.Rows.Add(c.Id, c.Nome, c.Email, cid.Estado.Sigla, c.Cidade.Nome, c.Endereco, c.Telefone);
                    }
                }
                catch
                {

                }
                #endregion
            }
        }

        private void btLimpar_Click(object sender, EventArgs e)
        {
            #region Limpar pesquisa
            check = false;
            carregarClientes();
            textFNome.Clear();
            comboFCidade.Text = "";
            #endregion
        }

        private void btPesquisar_Click(object sender, EventArgs e)
        {
            #region Carregar clientes que correspondam com os dados da pesquisa
            if (!textFNome.Text.Trim().Equals("") || !comboFCidade.Text.Trim().Equals(""))
            {
                check = true;
                carregarClientes();
            }
            #endregion
        }

        private void carregarInfCliente()
        {
            #region Carregar dados do cliente nos itens do groupBox
            try
            {
                Clientes c = clientesDAO.select(Convert.ToInt16(dataGridView.CurrentRow.Cells["ID"].Value.ToString()));
                textNome.Text = c.Nome;
                textNome.Focus();
                textEmail.Text = c.Email;

                if (c.Cpf == null)
                {
                    textDocumento.Text = c.Cnpj;
                    label2.Text = "CNPJ";
                }
                else
                {
                    textDocumento.Text = c.Cpf;
                    label2.Text = "CPF";
                }

                Cidades cid = cidadesDAO.selectCidade(c.Cidade.Id);
                textUF.Text = cid.Estado.Nome;

                textCidade.Text = c.Cidade.Nome;
                textEndereco.Text = c.Endereco;
                textTel.Text = c.Telefone;
                textTel2.Text = c.Telefone2;
            }
            catch
            {
                
            }
            #endregion
        }

        private void carregarInfObservacoes(int tipo)
        {
            idCliente = Convert.ToInt16(dataGridView.CurrentRow.Cells["Id"].Value.ToString());

            switch (tipo)
            {
                case 0:
                    #region Carregar informações das observações do cliente selecionado
                    listBoxData.Items.Clear();
                    listBoxObs.Items.Clear();
                    textObservacao.Text = "";

                    var lista = modelDB.Observacoes.GroupBy(x => new { x.Data, x.Cliente.Id }).Select(x => new { Data = x.Key.Data, Cliente = x.Key.Id }).Where(x => x.Cliente.ToString() != null && x.Cliente == idCliente).ToList();

                    foreach (var obs in lista)
                    {
                        listBoxData.Items.Add(obs.Data);
                    }
                    #endregion
                    break;
                case 1:
                    #region Carregar observações relacionadas com a data selecionada
                    try
                    {
                        listBoxObs.Items.Clear();
                        textObservacao.Text = "";

                        foreach (Observacoes obs in observacoesDAO.select().Where(x => x.Cliente != null && x.Cliente.Id == idCliente && x.Data == Convert.ToDateTime(listBoxData.Text)))
                        {
                            listBoxObs.Items.Add(obs.Id);
                        }
                    }
                    catch
                    {

                    }
                    #endregion
                    break;
                case 2:
                    #region Carregar texto da observação selecionada no textBox
                    try
                    {
                        foreach (Observacoes obs in observacoesDAO.select().Where(x => x.Id == Convert.ToInt16(listBoxObs.Text)))
                        {
                            textObservacao.Text = obs.Observacao;
                        }
                    }
                    catch
                    {

                    }
                    #endregion
                    break;
            }
        }

        private void btDetalhes_Click(object sender, EventArgs e)
        {
            if (dataGridView.Rows.Count >= 1)
            {
                #region Botão detalhes: mudar visibilidade e carregar informações do cliente
                tabControl.SelectedIndex = 0;
                groupBoxClientes.Visible = true;
                carregarInfCliente();
                #endregion
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    carregarInfCliente();
                    break;
                case 1:
                    carregarInfObservacoes(0);
                    break;
            }
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if(groupBoxClientes.Visible == true)
            {
                switch (tabControl.SelectedIndex)
                {
                    case 0:
                        carregarInfCliente();
                        break;
                    case 1:
                        carregarInfObservacoes(0);
                        break;
                }
            }
        }

        private void listBoxData_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region Carregar observações relacionadas à data selecionada [carregarInfObservacoes(1)]
            carregarInfObservacoes(1);

            if (listBoxObs.SelectedIndex == -1)
            {
                btAlterar.Enabled = false;
            }
            #endregion
        }

        private void listBoxObs_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region Carregar texto da observação selecionada [carregarInfObservacoes(2)]
            carregarInfObservacoes(2);

            if (listBoxObs.SelectedIndex >= 0)
            {
                btAlterar.Enabled = true;
            }
            #endregion
        }

        #region Auto Complete + ComboBox
        private void comboFCidade_DropDown(object sender, EventArgs e)
        {
            #region Habilitar auto complete mode e não deixar o dropdown list aparecer junto com a lista de sugestões
            // Once the user clicks on the DropDown button PreviewKeyDown event is attached to
            // that ComboBox. When user starts typing, freshly added event is triggered. In that
            // event we check if ComboBox is DroppedDown, if it is, focus that ComboBox. On
            // ComboBox focus DropDown disappeares and that's it.
            ComboBox cbo = (ComboBox)sender;
            cbo.PreviewKeyDown += new PreviewKeyDownEventHandler(comboFCidade_PreviewKeyDown);
            #endregion
        }

        // Código relacionado abaixo

        private void comboFCidade_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            #region Habilitar auto complete mode e não deixar o dropdown list aparecer junto com a lista de sugestões²
            ComboBox cbo = (ComboBox)sender;
            cbo.PreviewKeyDown -= comboFCidade_PreviewKeyDown;

            if (cbo.DroppedDown)
            {
                cbo.Focus();
            }
            #endregion
        }
        #endregion

        private void dataGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            #region Ao dar dois cliques em alguma "cell" do dataGridView, muda a visibilidade do groupBox
            if (e.RowIndex >= 0 && e.ColumnIndex <= 6)
            {
                tabControl.SelectedIndex = 0;
                groupBoxClientes.Visible = true;
                carregarInfCliente();
            }
            #endregion
        }

        #region Botões fechar
        private void btFechar_Click(object sender, EventArgs e)
        {
            #region Botão fechar: mudar visibilidade
            groupBoxClientes.Visible = false;
            btAlterar.Enabled = false;
            #endregion
        }

        private void btFechar2_Click(object sender, EventArgs e)
        {
            #region Botão fechar_2: mudar visibilidade
            groupBoxClientes.Visible = false;
            btAlterar.Enabled = false;
            #endregion
        }
        #endregion

        private void PesqClientes_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    #region Carregar clientes que correspondem com os dados da pesquisa ao apertar enter
                    if (!comboFCidade.DroppedDown)
                    {
                        if (!textFNome.Text.Trim().Equals("") || !comboFCidade.Text.Trim().Equals(""))
                        {
                            if (textFNome.Focused == true || comboFCidade.Focused == true)
                            {
                                check = true;
                                carregarClientes();
                            }
                        }
                    }
                    #endregion
                    break;
                case Keys.Escape:
                    #region Mudar visibilidade do groupBox ao apertar a tecla ESC
                    if(groupBoxClientes.Visible == true)
                    {
                        groupBoxClientes.Visible = false;
                        btAlterar.Enabled = false;
                    }
                    else
                    {
                        this.Close();
                    }
                    #endregion
                    break;
                case Keys.Delete:
                    #region Excluir observação selecionada ao apertar a tecla Delete
                    if (listBoxObs.SelectedIndex != -1 && listBoxObs.Focused == true)
                    {
                        excluirObs();
                    }
                    #endregion
                    break;
            }
        }

        private void excluirObs()
        {
            #region Exclusão de observação
            if (listBoxObs.SelectedIndex >= 0)
            {
                resposta = MessageBox.Show("Excluir a observação selecionada?", "Atenção", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (resposta == DialogResult.Yes)
                {
                    try
                    {
                        observacoesDAO.delete(Convert.ToInt16(listBoxObs.Text));
                        carregarInfObservacoes(0);
                        carregarInfObservacoes(1);
                        btAlterar.Enabled = false;
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione uma observação.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            #endregion
        }

        private void btExcluir_Click(object sender, EventArgs e)
        {
            excluirObs();
        }

        private void btAlterar_Click(object sender, EventArgs e)
        {
            #region Validação da observação
            errorProvider.SetError(textObservacao, string.Empty);

            if (textObservacao.Text.Trim().Equals(""))
            {
                errorProvider.SetError(textObservacao, "Informe uma observação");
                textObservacao.Focus();
                return;
            }
            #endregion

            #region Botão alterar: alterar o texto da observação selecionada
            try
            {
                observacoes = new Observacoes();
                observacoes.Id = Convert.ToInt16(listBoxObs.Text);
                observacoes.Observacao = textObservacao.Text.Trim();
                observacoesDAO.update(observacoes);

                listBoxObs.SelectedIndex = -1;
                textObservacao.Text = "";
            }
            catch
            {

            }
            #endregion
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender; // variável var = só pode ser declarada dentro de um método ou script

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewImageColumn && e.RowIndex >= 0 && e.ColumnIndex == 7)
            {
                #region Abrir form de envio de e-mail
                existe = false;
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm is EnvioDeEmail)
                    {
                        openForm.BringToFront();
                        existe = true;
                    }
                }
                if (!existe)
                {
                    envioDeEmail = new EnvioDeEmail(dataGridView.CurrentRow.Cells["Email"].Value.ToString());
                    envioDeEmail.MdiParent = this.ParentForm;
                    envioDeEmail.Show();
                }
                #endregion
            }
            else if (senderGrid.Columns[e.ColumnIndex] is DataGridViewImageColumn && e.RowIndex >= 0 && e.ColumnIndex == 8)
            {
                #region Abrir form para adicionar observação
                existe = false;
                foreach (Form openForm in Application.OpenForms)
                {
                    if (openForm is AddObservacao)
                    {
                        openForm.BringToFront();
                        existe = true;
                    }
                }
                if (!existe)
                {
                    addObservacao = new AddObservacao(1, dataGridView.CurrentRow.Cells["ID"].Value.ToString(), dataGridView.CurrentRow.Cells["Nome"].Value.ToString());
                    addObservacao.MdiParent = this.ParentForm;
                    addObservacao.Show();
                }
                #endregion
            }
        }
        private void btSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}