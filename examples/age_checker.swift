
func check_age(age) {
    set result = if (age >= 18) {
        "n adult"
    } else {
        " child"
    }
    "You are a" + result.to_upper()
}

func main() {
    set age = int(io.input("Enter an age >"))

    set response = check_age(age)
    io.println(response)

    set my_array = [1, 2, "test"]

    set my_obj = User("Tom", 19)
    io.println(my_obj.age())

    my_obj.set_age(235)

    io.println(my_obj.age())
}

func User(name, age) {
    set user = object.new()

    func user.age() {
        age
    }

    func user.set_age(new_age) {
        set age = new_age
    }

    func user.name() {
        name
    }
    
    user
}

//main()



set x = func(a, b) {
    a + b
}

io.println(x(1, 2))