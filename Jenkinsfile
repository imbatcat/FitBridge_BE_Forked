pipeline {
    agent {
        node {
            label 'docker-agent-dotnet'
        }
    }
    stages {
        stage('Build') {
            steps {
                echo "Building.."
                sh '''
                    pwd
                    dotnet restore
                '''
            }
        }
        stage('Test') {
            steps {
                echo "Testing.."
                sh '''
                    dotnet test
                '''
            }
        }
        stage('Build & Push') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'docker-credentials', 
                                          passwordVariable: 'PASS', 
                                          usernameVariable: 'USER')]) {
                    sh """
                        echo $PASS | docker login -u $USER --password-stdin
                        docker build -t rutkre/fitbridge-be:${env.BUILD_NUMBER} .
                        docker push rutkre/fitbridge-be:${env.BUILD_NUMBER}
                    """
                }
            }
        }
    }
}
