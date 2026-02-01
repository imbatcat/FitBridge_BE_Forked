pipeline {
    agent {
        node {
            label 'docker-agent-dotnet'
        }
    }
    stages {
        stage('Build') {
            steps {
                echo "Pulling.."
                pwd
                git --version
            }
        }
        stage('Test') {
            steps {
                echo "Testing.."
                sh '''
                echo "doing test stuff.."
                '''
            }
        }
        stage('Deliver') {
            steps {
                echo 'Deliver....'
                sh '''
                echo "doing delivery stuff.."
                '''
            }
        }
    }
}
